using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityMVC
{
    public class PendingViewResult : ActionResult
    {
        private Action<ActionResult> onFulfilled;
        public Action<ActionResult> OnFulfilled { get => onFulfilled; set => onFulfilled = value; }

        public PendingViewResult(string route)
        {
            base.RouteUrl = route;
        }
    }
}