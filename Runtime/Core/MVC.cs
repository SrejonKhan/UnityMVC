using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using System.Linq;
using UnityMVC.Utils;

namespace UnityMVC
{
    public static class MVC
    {
        private static GameObject mvcContainer;

        private static Canvas rootCanvas;
        public static Canvas RootCanvas { get => rootCanvas; }

        private static List<ActionResult> history = new List<ActionResult>();
        private static int currentHistIndex = 0;
        private static Dictionary<string, object[]> historyParams = new Dictionary<string, object[]>();

        private static ActionResult lastActiveView = null;

        private enum NavigateType { None, Forward, Backward };
        private static NavigateType navigateType = NavigateType.None;

        public static void Init(GameObject container, Canvas canvas)
        {
            ReflectionHelper.InitCache();
            mvcContainer = container;
            rootCanvas = canvas;
        }

        /// <summary>
        /// Get Result of valid route
        /// </summary>
        /// <param name="routeUrl">Route to view, e.g - Controller/Action/{data}</param>
        public static ActionResult Navigate(string routeUrl, params object[] args)
        {
            // sanity checks
            if (string.IsNullOrEmpty(routeUrl))
                throw new ArgumentNullException(nameof(routeUrl));

            // route data
            string[] routeData = routeUrl.Split('/');

            if (routeData.Length < 2)
                throw new ArgumentException("Invalid route param passed");

            string controllerName = routeData[0];
            string actionName = routeData[1];
            string data = routeData.Length > 2 ? routeData[2] : "";

            var controllerType = ReflectionHelper.GetControllerType(controllerName);

            // first, try to get instance from scene
            object controllerInstance = ReflectionHelper.MonoControllerInstances.ContainsKey(controllerType)
                ? ReflectionHelper.MonoControllerInstances[controllerType]
                : null;

            // if there is no instance in scene, we will make one. 
            if (controllerInstance == null)
            {
                // As we can't instance Mono Class with new keyword, 
                // we will inject a component into mvcContainer
                var comp = new GameObject(controllerName + "Controller").AddComponent(controllerType);
                comp.transform.parent = mvcContainer.transform;
                ReflectionHelper.MonoControllerInstances[controllerType] = comp;
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

            ActionResult result = null;

            // invoke action method
            result = (ActionResult)actionMethod.Invoke(controllerInstance, actionMethodParams.ToArray());
            result.OnResultInstantiated += OnViewInstantiated;

            result.RouteUrl = routeUrl;

            // add params to history
            if (historyParams.ContainsKey(routeUrl))
                historyParams[routeUrl] = args;
            else
                historyParams.Add(routeUrl, args);

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
                UnloadLastHistory(view);
        }

        /// <summary>
        /// Unload last history if exist, and, add next to History
        /// </summary>
        /// <param name="next">Next History (View)</param>
        public static void UnloadLastHistory(ActionResult next)
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
        /// Navigate backward to history
        /// </summary>
        /// <param name="steps">Steps to navigate back</param>
        public static void NavigateBackward(int steps)
        {
            navigateType = NavigateType.Backward;

            currentHistIndex = currentHistIndex - steps < 0 ? 0 : currentHistIndex - steps;

            var view = history[currentHistIndex];

            if (view.Equals(lastActiveView))
                return;

            string routeUrl = view.RouteUrl;

            object[] args = null;

            if (historyParams.ContainsKey(routeUrl))
                args = historyParams[routeUrl];

            Navigate(routeUrl, args);

        }

        /// <summary>
        /// Navigate forward to history 
        /// </summary>
        /// <param name="steps">Steps to navigate forward</param>
        public static void NavigateForward(int steps)
        {
            navigateType = NavigateType.Forward;

            currentHistIndex = currentHistIndex + steps >= history.Count ? history.Count - 1 : currentHistIndex + steps;

            var view = history[currentHistIndex];

            if (view.Equals(lastActiveView))
                return;

            string routeUrl = view.RouteUrl;

            object[] args = null;

            if (historyParams.ContainsKey(routeUrl))
                args = historyParams[routeUrl];

            Navigate(routeUrl, args);
        }
    }
}