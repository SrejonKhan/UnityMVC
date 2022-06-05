using System.Collections;
using UnityEngine;
using System.Reflection;
using System.Linq;
using UnityMVC.Utils;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using System.Collections.Generic;
using System;

namespace UnityMVC
{
    public static class MVC
    {
        private static GameObject mvcContainer;
        public static GameObject MvcContainer { get => mvcContainer; }

        private static GameObject root;
        public static GameObject Root { get => root; set => root = value; }

        public delegate void NavigateEventHandler(ActionResult ctx, ActionType type);
        public static event NavigateEventHandler OnNavigated;

        /// <summary>
        /// Initialize Method
        /// </summary>
        /// <param name="container">Container for Mvc Controller classes</param>
        /// <param name="canvas">Root Canvas of Game</param>
        public static void Init(GameObject container, GameObject rootGo,
            GameObject layout, bool makeLayoutRoot, UnityEvent<GameObject> onLayoutIntialized)
        {
            mvcContainer = container;
            root = rootGo;

            if (!root)
                throw new System.NullReferenceException("MVC Root cannot be null.");

            // load layout
            LayoutLoader.Load(layout, root.transform, makeLayoutRoot, onLayoutIntialized);

            // cache all MonoControllers and ViewClasses
            MvcReflection.InitCache();
        }

        /// <summary>
        /// Instantiate View of valid route
        /// </summary>
        /// <param name="routeUrl">Route to view, e.g - Controller/Action/{data}</param>
        public static ActionResult Navigate(string routeUrl, params object[] args)
        {
            return Route.Navigate(routeUrl, false, args);
        }

        /// <summary>
        /// Instantiate View of valid route
        /// </summary>
        /// <param name="routeUrl">Route to view, e.g - Controller/Action/{data}</param>
        /// <param name="partialView">True if partial</param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static ActionResult Navigate(string routeUrl, bool partialView, params object[] args)
        {
            return Route.Navigate(routeUrl, partialView, args);
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

        /// <summary>
        /// Get History of all Navigate() call
        /// </summary>
        /// <returns>View (ActionResult) Array</returns>
        public static ActionResult[] GetHistory()
        {
            return Route.GetHistory();
        }

        /// <summary>
        /// Get Last History
        /// </summary>
        /// <returns>View (ActionResult)</returns>
        public static ActionResult GetLastHistory()
        {
            return Route.GetLastHistory();
        }

        /// <summary>
        /// Invoke Event from Internal classes
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="type"></param>
        internal static void InvokeNavigateEvent(ActionResult ctx, ActionType type) 
            => OnNavigated?.Invoke(ctx, type);
    }
}