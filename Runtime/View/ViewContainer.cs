using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityMVC
{
    public class ViewContainer : MonoBehaviour
    {
        private object model;
        public object Model { get { return model; } set => model = value; }
        public ViewResult ViewResult { get; set; }

        public virtual ActionResult Navigate(string routeUrl)
        {
            return MVC.Navigate(routeUrl);
        }

        public virtual ActionResult NavigatePartial(string route)
        {
            return MVC.Navigate(route, true);
        }

        public virtual ActionResult NavigateBack()
        {
            return MVC.NavigateBackward(1);
        }

        public virtual ActionResult NavigateForward()
        {
            return MVC.NavigateForward(1);
        }

        public virtual void ReleaseAddressableReference()
        {
            ViewResult.ReleaseAddressableReference();
        }
    }
}

