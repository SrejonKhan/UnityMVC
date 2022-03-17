using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using System.Linq;
using Object = UnityEngine.Object;

namespace UnityMVC
{
    public class MVC
    {
        private static List<Type> monoControllerClasses = new List<Type>();
        private static Dictionary<Type, Object> monoControllerInstances = new Dictionary<Type, Object>();
        private static GameObject mvcContainer;

        private static List<Type> viewClasses = new List<Type>();

        // TODO: Call Init() from where? Another Mono Class or just upon any static method call?
        public static void Init(GameObject container) 
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (Assembly assembly in assemblies)
                monoControllerClasses.AddRange(GetMonoControllerClasses(assembly).ToList());

            foreach(var monoControllerClass in monoControllerClasses)
            {
                monoControllerInstances[monoControllerClass] = Object.FindObjectOfType(monoControllerClass);
            }

            foreach (Assembly assembly in assemblies)
                viewClasses.AddRange(GetViewClasses(assembly).ToList());

            mvcContainer = container;
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

            var controllerType = GetControllerType(controllerName);

            // first, try to get instance from scene
            object controllerInstance = monoControllerInstances.ContainsKey(controllerType) 
                ? monoControllerInstances[controllerType] 
                : null;
                                                                                 
            // if there is no instance in scene, we will make one. 
            if (controllerInstance == null)
            {
                // As we can't instance Mono Class with new keyword, 
                // we will inject a component into mvcContainer
                var comp = mvcContainer.AddComponent(controllerType);
                monoControllerInstances[controllerType] = comp;
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

            object result = null;

            // invoke action method
            result = actionMethod.Invoke(controllerInstance, actionMethodParams.ToArray());

            return (ActionResult)result;
        }

        /// <summary>
        /// Return Controller type
        /// </summary>
        /// <param name="controllerName">Controller name, without "Controller" suffix at end.</param>
        /// <returns>Type of Controller</returns>
        internal static Type GetControllerType(string controllerName)
        {
            return monoControllerClasses.
                Where(t => t.Name == controllerName + "Controller"). // XYZController
                FirstOrDefault();
        }

        /// <summary>
        /// Get all Controller Classes that derived from MonoController
        /// </summary>
        /// <param name="assembly">Assembly to lookup</param>
        /// <returns>IEnurable of Types</returns>
        private static IEnumerable<Type> GetMonoControllerClasses(Assembly assembly)
        {
            Type baseType = typeof(MonoController);
            return assembly.GetTypes().Where(t => t != baseType && baseType.IsAssignableFrom(t));
        }

        internal static Type GetViewType(string controllerName, string viewName)
        {
            // Refine controller and view name for lookup
            // Controller Name -> HomeController "{ControllerName}Controller", ends with "Controller" suffix (Pascal Case)
            // View Name -> HomeIndexView "{ControllerName}{ViewName}View", ends with "View" suffix (Pascal Case)
            controllerName = Char.ToUpperInvariant(controllerName[0]) + controllerName.Substring(1);
            viewName = Char.ToUpperInvariant(viewName[0]) + viewName.Substring(1);

            return viewClasses.
                Where(t => t.Name == $"{controllerName}{viewName}View"). // HomeIndexView
                FirstOrDefault();
        }

        /// <summary>
        /// Get all view classes
        /// </summary>
        /// <param name="assembly">Assembly to lookup</param>
        /// <returns>IEnurable of Types</returns>
        private static IEnumerable<Type> GetViewClasses(Assembly assembly)
        {
            Type baseType = typeof(ViewContainer);
            return assembly.GetTypes().Where(t => t != baseType && baseType.IsAssignableFrom(t));
        }

    }
}