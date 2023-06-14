using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityMVC
{
    public class ViewResult : ActionResult
    {
        private object model;
        private System.Type viewType;

        private ViewContainer viewContainerComponent;
        public ViewContainer ViewContainerComponent { get => viewContainerComponent; internal set => viewContainerComponent = value; }

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

            if (Parent == null && MVC.Root)
                Parent = MVC.Root.transform;

            // resolve
            await ExecuteResultAsync();
        }

        public override async Task ExecuteResultAsync()
        {
            base.OnResultInstantiated += OnViewInstantiate;

            // get result
            await base.ExecuteResultAsync();
        }

        private void OnViewInstantiate(ActionResult view)
        {
            base.OnResultInstantiated -= OnViewInstantiate;

            if (Result == null || view.InstantiatedObject == null) 
                return;

            GameObject resultGo = view.InstantiatedObject; // instantitated view, not prefab 

            viewType = MvcReflection.GetViewType(controllerName, viewName);

            if (viewType == null)
                throw new System.NullReferenceException($"No View class found for {viewName}.");

            // get or add view container (viewType)
            ViewContainerComponent = (ViewContainer)resultGo.GetComponent(viewType)
                ?? (ViewContainer)resultGo.AddComponent(viewType);

            // set model
            if (model != null)
                ViewContainerComponent.Model = model;

            // set view result
            ViewContainerComponent.ViewResult = this;

            /*---------------------INVOKE [INVOKE] ATTRIBUTES---------------------------*/
            var flags = (BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | 
                BindingFlags.Static | BindingFlags.DeclaredOnly); // flags for derived class method only

            // get all methods that implement [Invoke] Attribute
            MethodInfo[] invokableMethods = viewType.GetMethods(flags).
                Where(m => m.GetCustomAttributes(typeof(InvokeAttribute), false).Length > 0).
                ToArray();

            // invoke methods
            for (int i = 0; i < invokableMethods.Length; i++)
            {
                invokableMethods[i].Invoke(ViewContainerComponent, null); // no params are passed!
            }
        }

        public void Refresh()
        {
            if (viewType == null || ViewContainerComponent == null)
                return;

            var flags = (BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public |
                BindingFlags.Static | BindingFlags.DeclaredOnly); // flags for derived class method only

            MethodInfo[] invokableMethods = viewType.GetMethods(flags).
                Where(m => m.GetCustomAttributes(typeof(OnRefreshAttribute), false).Length > 0).
                ToArray();

            // invoke methods
            for (int i = 0; i < invokableMethods.Length; i++)
            {
                invokableMethods[i].Invoke(ViewContainerComponent, null); // no params are passed!
            }
        }
    }
}
