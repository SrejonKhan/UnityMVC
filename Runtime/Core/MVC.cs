using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using System.Linq;

namespace UnityMVC
{
    public static class MVC
    {
        private static GameObject mvcContainer;

        private static Canvas rootCanvas;
        public static Canvas RootCanvas { get => rootCanvas; }

        private static Dictionary<string, ActionResult> history = new Dictionary<string, ActionResult>();
        public static Dictionary<string, ActionResult> History { get => history; }

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
                var comp = mvcContainer.AddComponent(controllerType);
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

            // unload last of history
            UnloadLastHistory();

            // push to history
            history.Add(result.GetAddress(), result);

            return result;
        }

        public static void UnloadLastHistory()
        {
            var last = history.LastOrDefault();

            if (last.Equals(default(KeyValuePair<string, ActionResult>)))
                return;

            // destroy
            last.Value.Destroy();

            // release addressable reference
            last.Value.ReleaseReference(); 

            // No Remove as we want to keep a history?
            // history.Remove(last.Key);
        }

    }
}