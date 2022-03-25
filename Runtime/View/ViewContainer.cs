using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityMVC
{
    public class ViewContainer : MonoBehaviour
    {
        private object model;
        public object Model { get { return model; } set => model = value; }

        public virtual void Navigate(string routeUrl)
        {
            MVC.Navigate(routeUrl);
        }

        public virtual void NavigateBack()
        {
            MVC.NavigateBackward(1);
        }

        public virtual void NavigateForward()
        {
            MVC.NavigateForward(1);
        }
    }
}

