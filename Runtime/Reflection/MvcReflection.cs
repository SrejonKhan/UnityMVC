using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UnityMVC
{
    internal static class MvcReflection
    {
        private static List<Type> monoControllerClasses = new List<Type>();
        internal static List<Type> MonoControllerClasses { get => monoControllerClasses; }

        private static Dictionary<Type, Object> monoControllerInstances = new Dictionary<Type, Object>();
        internal static Dictionary<Type, Object> MonoControllerInstances { get => monoControllerInstances; }

        private static List<Type> viewClasses = new List<Type>();
        internal static List<Type> ViewClasses { get => viewClasses; }

        internal static void InitCache()
        {
            // clear
            monoControllerClasses.Clear();
            viewClasses.Clear();

            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            // get all MonoController
            foreach (Assembly assembly in assemblies)
                monoControllerClasses.AddRange(GetMonoControllerClasses(assembly).ToList());
            
            // get all ViewClass
            foreach (Assembly assembly in assemblies)
                viewClasses.AddRange(GetViewClasses(assembly).ToList());

            // get all MonoController class instance available in scene
            GetAllMonoControllerInstances();
        }

        private static void GetAllMonoControllerInstances()
        {
            foreach (var monoControllerClass in monoControllerClasses)
            {
                Object instance = MVC.MvcContainer.GetComponent(monoControllerClass);
                instance = instance != null ? instance : MVC.MvcContainer.GetComponentInChildren(monoControllerClass);
                instance = instance != null ? instance : Object.FindObjectOfType(monoControllerClass);

                monoControllerInstances[monoControllerClass] = instance;
            }
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