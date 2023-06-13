using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using MiddlewareDelegate = System.Func<UnityMVC.ActionResult, UnityMVC.ActionType, bool>;

namespace UnityMVC
{
    public class MiddlewareConfiguration
    {
        private Dictionary<string, MiddlewareDelegate> middlewares = new Dictionary<string, MiddlewareDelegate>();
        private List<string> middlewareRoutes = new List<string>();

        public void OnRoute(string route, MiddlewareDelegate callback)
        {
            if (route == null)
                throw new ArgumentNullException(nameof(route));

            if (callback == null)
                return;

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
            // we presume we don't call unless checking beforehand
            // still, it's a simple protection. 
            if (!middlewares.ContainsKey(route)) 
                return false;

            return middlewares[route].Invoke(ctx, type);
        }

        internal bool HasMiddlewareRegistered(string route)
        {
            return middlewares.ContainsKey(route);
        }

        internal bool IsMiddlewareValid(string route)
        {
            for(int i = 0; i < middlewareRoutes.Count; i++)
            {
                bool isValid = MatchWildcard(route, middlewareRoutes[i]);
                if (isValid) return true;
            }
            return false;
        }

        private bool MatchWildcard(string inputRoute, string route)
        {
            if (inputRoute == "*")
            {
                // If the inputRoute is "*", it matches all paths
                return true;
            }

            if (inputRoute.EndsWith("/*"))
            {
                // If the inputRoute ends with "/*", match all paths that start with the same prefix
                string prefix = inputRoute.Substring(0, inputRoute.Length - 2);
                return route.StartsWith(prefix);
            }

            inputRoute = inputRoute.Replace(".", @"\.").Replace("*", ".*");

            return Regex.IsMatch(route, $"^{inputRoute}$");
        }
    }
}
