using System;
using System.Collections.Generic;
using MiddlewareDelegate = System.Func<UnityMVC.ActionResult, UnityMVC.ActionType, bool>;
using MiddlewareDelegateList = System.Collections.Generic.List<System.Func<UnityMVC.ActionResult, UnityMVC.ActionType, bool>>;

namespace UnityMVC
{
    public class MiddlewareConfiguration
    {
        private Dictionary<string, MiddlewareDelegateList> middlewares = new Dictionary<string, MiddlewareDelegateList>();
        private Dictionary<string, MiddlewareDelegateList> controllerSpecificAllMiddlewares = new Dictionary<string, MiddlewareDelegateList>();
        private MiddlewareDelegateList allRouteMiddleware = new MiddlewareDelegateList();
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
                allRouteMiddleware.Add(callback);
                return;
            }

            // controller specific all view route wildcard
            // E.g. - Home/*
            if(route.EndsWith("/*"))
            {
                string controllerName = route.Substring(0, route.Length - 2);
                if(controllerSpecificAllMiddlewares.ContainsKey(controllerName))
                    controllerSpecificAllMiddlewares[controllerName].Add(callback);
                else 
                    controllerSpecificAllMiddlewares.Add(controllerName, new MiddlewareDelegateList { callback });
                return;
            }

            if (middlewares.ContainsKey(route))
                middlewares[route].Add(callback);
            else
            {
                middlewares.Add(route, new MiddlewareDelegateList { callback });
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
            {
                bool tmpResult = false;
                for (int i = 0; i < allRouteMiddleware.Count; i++)
                {
                    MiddlewareDelegate middleware = allRouteMiddleware[i];
                    tmpResult = middleware.Invoke(ctx, type);
                    if (!tmpResult) break;
                }
                result = tmpResult;
            }
            
            if (!result) return result; // if false, immediately return false

            // route specific wildcard - "Home/*"
            string routeController = route.Split('/')[0];
            if(controllerSpecificAllMiddlewares.ContainsKey(routeController))
            {
                bool tmpResult = false;
                MiddlewareDelegateList middlewareDelegates = controllerSpecificAllMiddlewares[routeController];
                for(int i = 0; i < middlewareDelegates.Count; i++)
                {
                    tmpResult = middlewareDelegates[i].Invoke(ctx, type);
                    if (!tmpResult) break;
                }
                result = tmpResult;
            }
            if (!result) return result; // if false, immediately return false

            // route specified
            if (middlewares.ContainsKey(route))
            {
                bool tmpResult = false;
                MiddlewareDelegateList middlewareDelegates = middlewares[route];
                for(int i = 0; i < middlewareDelegates.Count; i++)
                {
                    tmpResult = middlewareDelegates[i].Invoke(ctx, type);
                    if(!tmpResult) break;
                }
                result = tmpResult;
            }

            return result;
        }

        internal bool HasMiddlewareRegistered(string route)
        {
            string routeController = route.Split('/')[0];
            
            return middlewares.ContainsKey(route) // if any middleware registered for this route
                || controllerSpecificAllMiddlewares.ContainsKey(routeController) // if middleware registered for controller route
                || allRouteMiddleware.Count > 0; // if all route middleware registered
        }
    }
}
