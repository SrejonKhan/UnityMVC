using System.Collections.Generic;
using UnityEngine;
using System;

namespace UnityMVC
{
    internal static class Route
    {
        private static ActionResult lastActiveView = null;

        private static List<ActionResult> history = new List<ActionResult>();
        private static int currentHistIndex = 0;
        private static Dictionary<string, object[]> historyArgs = new Dictionary<string, object[]>();

        private enum NavigateType { None, Forward, Backward };
        private static NavigateType navigateType = NavigateType.None;

        /// <summary>
        /// Get Result of valid route
        /// </summary>
        /// <param name="routeUrl">Route to view, e.g - Controller/Action/{data}</param>
        internal static ActionResult Navigate(string routeUrl, bool partialView, bool pushToHistory, params object[] args)
        { 
            return ExecuteNavigation(routeUrl, partialView, pushToHistory, args);
        }

        /// <summary>
        /// Navigate backward to history
        /// </summary>
        /// <param name="steps">Steps to navigate back</param>
        internal static ActionResult NavigateBackward(int steps)
        {
            if (history.Count == 0)
                return new FailedViewResult("");

            navigateType = NavigateType.Backward;

            currentHistIndex = currentHistIndex - steps < 0 ? 0 : currentHistIndex - steps;

            var view = history[currentHistIndex];

            if (view.Equals(lastActiveView))
            {
                navigateType = NavigateType.None;
                return new FailedViewResult(GetCurrentHist().RouteUrl);
            }

            string routeUrl = view.RouteUrl;

            object[] args = null;

            if (historyArgs.ContainsKey(routeUrl))
                args = historyArgs[routeUrl];

            return ExecuteNavigation(routeUrl, false, false, args);
        }

        /// <summary>
        /// Navigate forward to history 
        /// </summary>
        /// <param name="steps">Steps to navigate forward</param>
        internal static ActionResult NavigateForward(int steps)
        {
            if (history.Count == 0)
                return new FailedViewResult("");

            navigateType = NavigateType.Forward;

            currentHistIndex = currentHistIndex + steps >= history.Count ? history.Count - 1 : currentHistIndex + steps;

            var view = history[currentHistIndex];

            if (view.Equals(lastActiveView))
            {
                navigateType = NavigateType.None;
                return new FailedViewResult(GetCurrentHist().RouteUrl);
            }

            string routeUrl = view.RouteUrl;

            object[] args = null;

            if (historyArgs.ContainsKey(routeUrl))
                args = historyArgs[routeUrl];

            return ExecuteNavigation(routeUrl, false, false, args);
        }

        /// <summary>
        /// Execute Navigation by finding proper model, view, and controller.
        /// </summary>
        /// <param name="routeUrl">Route Url to execute on</param>
        /// <param name="args">Optional args to pass to Controller</param>
        /// <returns>View as ActionResult object</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        private static ActionResult ExecuteNavigation(string routeUrl, bool partialView, bool pushToHistory, params object[] args)
        {
            // sanity checks
            if (string.IsNullOrEmpty(routeUrl))
                throw new ArgumentNullException(nameof(routeUrl));

            // route data
            string[] routeData = routeUrl.Split('/');
            if (routeData.Length < 2)
                throw new ArgumentException("Invalid route param passed");

            ActionResult result = new PendingViewResult(routeUrl);
            var actionType = partialView ? ActionType.PartialView : ActionType.View;

            // invoke BeforeNavigate event
            bool shouldContinue = MVC.InvokeBeforeNavigateEvent(result, actionType);

            // invoke middleware
            var middleware = MVC.ConfigureMiddleware();
            if (shouldContinue && middleware.HasMiddlewareRegistered(routeUrl))
            {
                shouldContinue = MVC.ConfigureMiddleware().InvokeMiddleware(routeUrl, result, actionType);
            }

            if (!shouldContinue) return new FailedViewResult(routeUrl);

            string controllerName = routeData[0];
            string actionName = routeData[1];
            string data = routeData.Length > 2 ? routeData[2] : "";

            var controllerType = MvcReflection.GetControllerType(controllerName);

            // first, try to get instance from scene
            object controllerInstance = MvcReflection.MonoControllerInstances.ContainsKey(controllerType)
                ? MvcReflection.MonoControllerInstances[controllerType]
                : null;

            // if there is no instance in scene, we will make one. 
            if (controllerInstance == null)
            {
                // As we can't instance Mono Class with new keyword, 
                // we will inject a component into mvcContainer
                var comp = new GameObject(controllerName + "Controller").AddComponent(controllerType);
                comp.transform.parent = MVC.MvcContainer.transform;
                MvcReflection.MonoControllerInstances[controllerType] = comp;
                controllerInstance = comp;
            }

            // call action method
            var actionMethod = controllerType.GetMethod(actionName);
            var actionMethodParamsInfo = actionMethod.GetParameters();
            List<object> actionMethodParams = new List<object>();

            // we have data from route
            if (data != "")
            {
                Type dataParamType = actionMethodParamsInfo[0].ParameterType;
                actionMethodParams.Add(Convert.ChangeType(data, dataParamType));
            }
            // add args as param
            actionMethodParams.AddRange(args);

            // any middleware may be waiting for actual view result
            Action<ActionResult> onResultFulfilled = ((PendingViewResult)result).OnFulfilled;

            // invoke action method
            result = null;
            result = (ActionResult)actionMethod.Invoke(controllerInstance, actionMethodParams.ToArray());

            bool isActionMethodFailed = false;
            if (result == null) 
            {
                isActionMethodFailed = true;
                result = new FailedViewResult(routeUrl);
            }

            onResultFulfilled?.Invoke(result);
            result.RouteUrl = routeUrl;
            result.NavigationActionType = actionType;

            if (isActionMethodFailed) return result;

#if !UNITY_WEBGL
            if (!partialView) result.OnResultInstantiated += OnViewInstantiated;
#else
            if (!partialView) OnViewInstantiated(result); // synchronous call
#endif
            // no need to keep history if it's partial or directed not to keep
            if (partialView || !pushToHistory)
            {
                // invoke Navigate Event
                MVC.InvokeNavigateEvent(result, partialView ? ActionType.PartialView : ActionType.View);
                return result;
            }

            // add args to history
            if (historyArgs.ContainsKey(routeUrl))
                historyArgs[routeUrl] = args;
            else
                historyArgs.Add(routeUrl, args);

            // invoke Navigate Event
            MVC.InvokeNavigateEvent(result, partialView ? ActionType.PartialView : ActionType.View);

            return result;
        }

        /// <summary>
        /// On Current/Next view result instantiated
        /// </summary>
        /// <param name="view"></param>
        private static void OnViewInstantiated(ActionResult view)
        {
            view.OnResultInstantiated -= OnViewInstantiated;

            // unload last of history
            if (view != null)
                UnloadLastView(view);
        }

        /// <summary>
        /// Unload last history if exist, and, add next to History
        /// </summary>
        /// <param name="next">Next History (View)</param>
        public static void UnloadLastView(ActionResult next)
        {
            // first navigation
            if (lastActiveView == null)
            {
                history.Add(next);
                lastActiveView = next;
                return;
            }

            // destroy previous view
            lastActiveView.Destroy();
            // release addressable reference
            lastActiveView.ReleaseAddressableReference();

            // set current view to lastActiveView
            lastActiveView = next;

            // return if it's backward or forward navigation, because they don't need history updation
            if (navigateType == NavigateType.Forward || navigateType == NavigateType.Backward)
            {
                navigateType = NavigateType.None;
                return;
            }

            // history updation
            if(currentHistIndex < history.Count - 1) // if we are not in the last history,  
            {
                // remove all history in front of current history
                history.RemoveRange(currentHistIndex + 1, history.Count - (currentHistIndex + 1)); 
            }
            history.Add(next);
            currentHistIndex++;
        }

        /// <summary>
        /// Get History
        /// </summary>
        /// <returns>Views Array</returns>
        internal static ActionResult[] GetHistory()
        {
            return history.ToArray();
        }

        /// <summary>
        /// [DO NOT USE TO MODIFY HISTORY] Get History List Reference
        /// </summary>
        /// <returns>Action Result List</returns>
        internal static List<ActionResult> DANGEROUS_GetHistoryList()
        {
            return history;
        }

        /// <summary>
        /// [DO NOT USE TO MODIFY HISTORY ARGs] Get history args reference
        /// </summary>
        /// <returns>Dictionary of args</returns>
        internal static Dictionary<string, object[]> DANGEROUS_GetHistoryArgs()
        {
            return historyArgs;
        }

        /// <summary>
        /// Get Last History
        /// </summary>
        /// <returns>View</returns>
        internal static ActionResult GetLastHistory()
        {
            if(history.Count == 0) return null;

            return history[history.Count - 1];
        }

        /// <summary>
        /// Clear all history
        /// </summary>
        internal static void ClearHistory()
        {
            history.Clear();
            historyArgs.Clear();

            // currentHistIndex is set to -1 because in the next navigation
            // it will be incremented to 0. 
            // If we set to 0, in next navigation, it will be incremented to 1
            // where it should be 0 as it's the first fresh navigation. 
            currentHistIndex = -1; 
        }

        /// <summary>
        /// Get Current History Index
        /// </summary>
        /// <returns>History Index</returns>
        internal static int GetCurrentHistIndex()
        {
            return currentHistIndex;
        }

        /// <summary>
        /// Get Current History 
        /// </summary>
        /// <returns>ActionResult of current history</returns>
        internal static ActionResult GetCurrentHist()
        {
            return history[currentHistIndex];
        }
    }
}