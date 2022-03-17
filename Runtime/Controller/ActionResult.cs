using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace UnityMVC
{
    public abstract class ActionResult : IActionResult
    {
        private protected string controllerName;
        private protected string viewName;

        private Object result;
        public Object Result { get => result; private protected set => result = value; }

        private Transform parent;
        public Transform Parent { get => parent; private protected set => parent = value; }

        private ViewContainer viewContainer;
        public ViewContainer ViewContainer { get => viewContainer; private protected set => viewContainer = value; }

        private AsyncOperationHandle handle;
        public AsyncOperationHandle Handle { get => handle; }

        public virtual async Task ExecuteResultAsync()
        {
            string location = $"{controllerName}/{viewName}";
            //Result = (Object)Addressables.LoadAssetAsync<GameObject>(location);
            handle = await AddressableLoader.LoadAssetAsync<GameObject>(location);

            Result = (GameObject)handle.Result;

            if (Result == null)
                throw new System.ArgumentNullException("Result", $"Couldn't find view at location - " +
                    $"/Resources/{location}");

            Instantiate();
        }

        /// <summary>
        /// Instantiate Result as GameObject
        /// </summary>
        /// <param name="setParent">Set instantiated GameObject to available parent, if available.</param>
        /// <returns>Instantiated GameObject</returns>
        private GameObject Instantiate(bool setParent = true)
        {
            GameObject go = null;

            if (setParent && parent)
                go = Object.Instantiate((GameObject)Result, parent);
            else
                go = Object.Instantiate((GameObject)Result);
            
            return go;
        }
    }
}