using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityMVC
{
    public class ViewContainer : MonoBehaviour
    {
        private object model;
        public object Model { get { return model; } set => model = value; }

        public void Navigate(string routeUrl)
        {
            MVC.Navigate(routeUrl);
        }

        public void NavigateBack()
        {
            MVC.NavigateBackward(1);
        }

        public void NavigateForward()
        {
            MVC.NavigateForward(1);
        }
    }
}

