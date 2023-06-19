using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using MiddlewareDelegate = System.Func<UnityMVC.ActionResult, UnityMVC.ActionType, bool>;

namespace UnityMVC
{
    public class MiddlewareConfiguration
    {
        private Dictionary<string, MiddlewareDelegate> middlewares = new Dictionary<string, MiddlewareDelegate>();
        private Dictionary<string, MiddlewareDelegate> controllerSpecificAllMiddlewares = new Dictionary<string, MiddlewareDelegate>();
        private MiddlewareDelegate allRouteMiddleware;
        private List<string> middlewareRoutes = new List<string>();

        public void OnRoute(string route, MiddlewareDelegate callback)
        {
            if (route == null)
                throw new ArgumentNullException(nameof(route));

            if (callback == null)
                return;

            // all route wildcard
            if(route == "*") 
            {
                allRouteMiddleware = callback;
                return;
            }

            // controller specific all view route wildcard
            // E.g. - Home/*
            if(route.EndsWith("/*"))
            {
                string controllerName = route.Substring(0, route.Length - 2);
                if(controllerSpecificAllMiddlewares.ContainsKey(controllerName))
                    controllerSpecificAllMiddlewares[controllerName] = callback;
                else 
                    controllerSpecificAllMiddlewares.Add(controllerName, callback);
                return;
            }

            if (middlewares.ContainsKey(route))
                middlewares[route] = callback;
            else
            {
                middlewares.Add(route, callback);
                middlewareRoutes.Add(route);
            }
        }

        public void RemoveRouteConfiguration(string route)
        {
            if(middlewares.ContainsKey(route)) 
            {
                middlewares.Remove(route);
                middlewareRoutes.Remove(route);
            }
        }

        internal bool InvokeMiddleware(string route, ActionResult ctx, ActionType type)
        {
            bool result = false;
            
            // all route = "*" wildcard
            if(allRouteMiddleware != null) 
                result = allRouteMiddleware.Invoke(ctx, type);

            if (!result) return result; // if false, immediately return false

            // route specific wildcard - "Home/*"
            string routeController = route.Substring(0, route.Length - 2);
            if(controllerSpecificAllMiddlewares.ContainsKey(routeController))
                result = controllerSpecificAllMiddlewares[routeController].Invoke(ctx, type);

            if (!result) return result; // if false, immediately return false

            if (middlewares.ContainsKey(route)) 
                result = middlewares[route].Invoke(ctx, type);

            return result;
        }

        internal bool HasMiddlewareRegistered(string route)
        {
            string routeController = route.Substring(0, route.Length - 2);
            
            return middlewares.ContainsKey(route) // if any middleware registered for this route
                || controllerSpecificAllMiddlewares.ContainsKey(routeController) // if middleware registered for controller route
                || allRouteMiddleware != null; // if all route middleware registered
        }
    }
}
