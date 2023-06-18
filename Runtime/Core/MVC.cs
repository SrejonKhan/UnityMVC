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
        public delegate bool BeforeNavigateEventHandler(ActionResult ctx, ActionType type);
        public static event BeforeNavigateEventHandler BeforeNavigate;

        private static MiddlewareConfiguration middlewareConfiguration;

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
            return Route.Navigate(routeUrl, false, true, args);
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
            return Route.Navigate(routeUrl, partialView, true, args);
        }

        /// <summary>
        /// Instantiate View of valid route without pushing to history
        /// </summary>
        /// <param name="routeUrl">Route to view, e.g - Controller/Action/{data}</param>
        /// <param name="partialView">True if partial</param>
        /// <param name="pushToHistory">If push to history</param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static ActionResult Navigate(string routeUrl, bool partialView, bool pushToHistory, params object[] args)
        {
            return Route.Navigate(routeUrl, partialView, pushToHistory, args);
        }

        /// <summary>
        /// Navigate backward to history
        /// </summary>
        /// <param name="steps">Steps to navigate back</param>
        public static ActionResult NavigateBackward(int steps)
        {
            return Route.NavigateBackward(steps);
        }

        /// <summary>
        /// Navigate forward to history 
        /// </summary>
        /// <param name="steps">Steps to navigate forward</param>
        public static ActionResult NavigateForward(int steps)
        {
            return Route.NavigateForward(steps);
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
        /// Clear History (Beta)
        /// </summary>
        public static void ClearHistory()
        {
            Route.ClearHistory();
        }

        /// <summary>
        /// Get Current History Index
        /// </summary>
        /// <returns>History Index</returns>
        public static int GetCurrentHistoryIndex()
        {
            return Route.GetCurrentHistIndex();
        }

        /// <summary>
        /// Get Current History
        /// </summary>
        /// <returns>ActionResult of current history </returns>
        public static ActionResult GetCurrentHistory()
        {
            return Route.GetCurrentHist();
        }

        /// <summary>
        /// [DO NOT USE TO MODIFY HISTORY] Get History List Reference
        /// </summary>
        /// <returns>Action Result List</returns>
        public static List<ActionResult> DANGEROUS_GetHistoryList()
        {
            return Route.DANGEROUS_GetHistoryList();
        }

        /// <summary>
        /// [DO NOT USE TO MODIFY HISTORY PARAMs] Get history args reference
        /// </summary>
        /// <returns>Dictionary of params</returns>
        public static Dictionary<string, object[]> DANGEROUS_GetHistoryArgs()
        {
            return Route.DANGEROUS_GetHistoryArgs();
        }

        /// <summary>
        /// Invoke Event from Internal classes
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="type"></param>
        internal static void InvokeNavigateEvent(ActionResult ctx, ActionType type) 
            => OnNavigated?.Invoke(ctx, type);

        /// <summary>
        /// Invoke Event from Internal classes
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="type"></param>
        internal static bool InvokeBeforeNavigateEvent(ActionResult ctx, ActionType type)
            => BeforeNavigate != null ? BeforeNavigate.Invoke(ctx, type) : true;

        public static MiddlewareConfiguration ConfigureMiddleware()
        {
            if(middlewareConfiguration == null)
                middlewareConfiguration = new MiddlewareConfiguration();

            return middlewareConfiguration;
        }

        public static void ClearMiddleware()
        {
            middlewareConfiguration = null;
        }
    }
}