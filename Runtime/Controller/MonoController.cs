using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace UnityMVC
{
    public class MonoController : MonoBehaviour
    {
        public ViewResult View([CallerMemberName] string viewName = null)
        {
            return new ViewResult(GetControllerName(), viewName);
        }

        public ViewResult View(Transform parent, [CallerMemberName] string viewName = null)
        {
            return new ViewResult(parent, null, GetControllerName(), viewName);
        }
        
        public ViewResult View(object model, [CallerMemberName] string viewName = null)
        {
            return new ViewResult(model, GetControllerName(), viewName);
        }

        public ViewResult View(Transform parent, object model, [CallerMemberName] string viewName = null)
        {
            return new ViewResult(parent, model, GetControllerName(), viewName);
        }

        private string GetControllerName() 
        {
            return GetType().Name.Replace("Controller", "");
        }
    }
}
