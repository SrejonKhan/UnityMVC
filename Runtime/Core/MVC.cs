using System.Collections;
using UnityEngine;
using System.Reflection;
using System.Linq;
using UnityMVC.Utils;

namespace UnityMVC
{
    public static class MVC
    {
        private static GameObject mvcContainer;
        public static GameObject MvcContainer { get => mvcContainer; }

        private static Canvas rootCanvas;
        public static Canvas RootCanvas { get => rootCanvas; }

        /// <summary>
        /// Initialize Method
        /// </summary>
        /// <param name="container">Container for Mvc Controller classes</param>
        /// <param name="canvas">Root Canvas of Game</param>
        public static void Init(GameObject container, Canvas canvas)
        {
            mvcContainer = container;
            rootCanvas = canvas;

            MvcReflection.InitCache();
        }

        /// <summary>
        /// Get Result of valid route
        /// </summary>
        /// <param name="routeUrl">Route to view, e.g - Controller/Action/{data}</param>
        public static ActionResult Navigate(string routeUrl, params object[] args)
        {
            return Route.Navigate(routeUrl, args);
        }

        /// <summary>
        /// Navigate backward to history
        /// </summary>
        /// <param name="steps">Steps to navigate back</param>
        public static void NavigateBackward(int steps)
        {
            Route.NavigateBackward(steps);
        }

        /// <summary>
        /// Navigate forward to history 
        /// </summary>
        /// <param name="steps">Steps to navigate forward</param>
        public static void NavigateForward(int steps)
        {
            Route.NavigateForward(steps);
        }
    }
}