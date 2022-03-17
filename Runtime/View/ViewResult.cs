using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityMVC
{
    public class ViewResult : ActionResult
    {
        private object model;

        public ViewResult(string controllerName, string viewName = null)
        {
            _ = InitAsync(controllerName, viewName);
        }

        public ViewResult(object model, string controllerName, string viewName = null)
        {
            this.model = model;
            _ = InitAsync(controllerName, viewName);
        }

        // constructor chaining cannot be as Init() should be called after parent and model is set
        public ViewResult(Transform parent, object model, string controllerName, string viewName = null)
        {
            Parent = parent;
            this.model = model;
            _ = InitAsync(controllerName, viewName);
        }


        private async Task InitAsync(string controllerName, string viewName = null)
        {
            // sanity checks
            if (string.IsNullOrEmpty(viewName))
                throw new System.ArgumentNullException(viewName);

            this.controllerName = controllerName;
            this.viewName = viewName;

            // resolve
            await ExecuteResultAsync();
        }

        public override async Task ExecuteResultAsync()
        {
            // get result
            await base.ExecuteResultAsync();

            if (Result == null || model == null) return;

            GameObject resultGo = (GameObject)Result;
            System.Type viewType = MVC.GetViewType(controllerName, viewName);

            if (viewType == null)
                throw new System.NullReferenceException($"No View class found for {viewName}.");

            // get or add view container (viewType)
            ViewContainer viewContainer = (ViewContainer)resultGo.GetComponent(viewType) 
                ?? (ViewContainer)resultGo.AddComponent(viewType);

            // set model
            if (model != null)
                viewContainer.Model = model;

            // invoke all methods with "Invoke" attributes
            var flags = (BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);

            MethodInfo[] invokableMethods = viewType.GetMethods(flags).
                Where(m => m.GetCustomAttributes(typeof(InvokeAttribute), false).Length > 0).
                ToArray();

            for (int i = 0; i < invokableMethods.Length; i++)
            {
                invokableMethods[i].Invoke(viewContainer, null);
            }
        }
    }
}
