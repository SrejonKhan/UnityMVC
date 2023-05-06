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
        private static Dictionary<string, object[]> historyParams = new Dictionary<string, object[]>();

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
        internal static void NavigateBackward(int steps)
        {
            if (history.Count == 0)
                return;

            navigateType = NavigateType.Backward;

            currentHistIndex = currentHistIndex - steps < 0 ? 0 : currentHistIndex - steps;

            var view = history[currentHistIndex];

            if (view.Equals(lastActiveView))
                return;

            string routeUrl = view.RouteUrl;

            object[] args = null;

            if (historyParams.ContainsKey(routeUrl))
                args = historyParams[routeUrl];

            ExecuteNavigation(routeUrl, false, true, args);
        }

        /// <summary>
        /// Navigate forward to history 
        /// </summary>
        /// <param name="steps">Steps to navigate forward</param>
        internal static void NavigateForward(int steps)
        {
            if (history.Count == 0)
                return;

            navigateType = NavigateType.Forward;

            currentHistIndex = currentHistIndex + steps >= history.Count ? history.Count - 1 : currentHistIndex + steps;

            var view = history[currentHistIndex];

            if (view.Equals(lastActiveView))
                return;

            string routeUrl = view.RouteUrl;

            object[] args = null;

            if (historyParams.ContainsKey(routeUrl))
                args = historyParams[routeUrl];

            ExecuteNavigation(routeUrl, false, true, args);
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
            bool shouldContinue = MVC.InvokeBeforeNavigateEvent(result, partialView ? ActionType.PartialView : ActionType.View);

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

            // invoke action method
            result = (ActionResult)actionMethod.Invoke(controllerInstance, actionMethodParams.ToArray());
            if (result == null)
                return new FailedViewResult(routeUrl);

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

            // add params to history
            if (historyParams.ContainsKey(routeUrl))
                historyParams[routeUrl] = args;
            else
                historyParams.Add(routeUrl, args);

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
            lastActiveView.ReleaseReference();

            // set current view to lastActiveView
            lastActiveView = next;

            // return if it's backward or forward navigation
            if (navigateType == NavigateType.Forward || navigateType == NavigateType.Backward)
            {
                navigateType = NavigateType.None;
                return;
            }

            //history.RemoveRange(currentHistIndex + 1, history.Count - (currentHistIndex + 1));
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
            historyParams.Clear();
            currentHistIndex = 0;
        }

        /// <summary>
        /// Get Current History Index
        /// </summary>
        /// <returns>History Index</returns>
        internal static int GetCurrentHistIndex()
        {
            return currentHistIndex;
        }
    }
}