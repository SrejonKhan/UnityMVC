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

        public virtual void Navigate(string routeUrl)
        {
            MVC.Navigate(routeUrl);
        }

        public virtual void NavigatePartial(string route)
        {
            MVC.Navigate(route, true);
        }

        public virtual void NavigateBack()
        {
            MVC.NavigateBackward(1);
        }

        public virtual void NavigateForward()
        {
            MVC.NavigateForward(1);
        }

        public virtual ActionResult NavigateActionResult(string routeUrl)
        {
            return MVC.Navigate(routeUrl);
        }

        public virtual ActionResult NavigatePartialActionResult(string route)
        {
            return MVC.Navigate(route, true);
        }

        public virtual ActionResult NavigateBackActionResult()
        {
            return MVC.NavigateBackward(1);
        }

        public virtual ActionResult NavigateForwardActionResult()
        {
            return MVC.NavigateForward(1);
        }

        public virtual void ReleaseAddressableReference()
        {
            ViewResult.ReleaseAddressableReference();
        }
    }
}

