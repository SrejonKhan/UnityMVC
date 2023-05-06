using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityMVC
{
    public class PendingViewResult : ActionResult
    {
        public PendingViewResult(string route)
        {
            base.RouteUrl = route;
        }
    }
}