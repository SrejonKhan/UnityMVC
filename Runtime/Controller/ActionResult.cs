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

        private AsyncOperationHandle handle;
        public AsyncOperationHandle Handle { get => handle; }

        private GameObject instantiatedObject;
        public GameObject InstantiatedObject { get => instantiatedObject; }

        public delegate void ResultInstantiated(ActionResult view);
        public event ResultInstantiated OnResultInstantiated;

        private string routeUrl;
        public string RouteUrl { get => routeUrl; internal set => routeUrl = value; }

        private bool isFailed;
        public bool IsFailed { get => isFailed; internal set => isFailed = value; }

        private ActionType navigationActionType;
        public ActionType NavigationActionType { get => navigationActionType; internal set => navigationActionType = value; }

        public virtual async Task ExecuteResultAsync()
        {
            string address = GetAddress();

            // Asynchronous for non WebGl platform
            // Synchronous for WebGL as it doesn't support multithreading
#if !UNITY_WEBGL
            handle = await AddressableLoader.LoadAssetAsync<GameObject>(address);

            if (handle.Result == null)
                throw new System.ArgumentNullException("Result", $"Couldn't find view at address - " +
                    $"{address}");

            result = (GameObject)handle.Result;
#else
            result = AddressableLoader.LoadAsset<GameObject>(address);

            if (result == null)
                throw new System.ArgumentNullException("Result", $"Couldn't find view at address - " +
                    $"{address}");

#endif
            instantiatedObject = Instantiate();
            OnResultInstantiated?.Invoke(this); // invoke related event
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

        internal bool Destroy()
        {
            if (instantiatedObject == null)
                return false;
            try 
            {
                UnityEngine.Object.Destroy(instantiatedObject);
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError("Could not Destroy Action Result - " + e.Message);
                return false;
            }
        }

        public void ReleaseAddressableReference()
        {
            if (!handle.IsValid())
                return;

            Addressables.Release(handle);
        }

        internal string GetAddress()
        {
            if(string.IsNullOrEmpty(controllerName) || string.IsNullOrEmpty(viewName))
                return string.Empty;

            string address = $"{controllerName}/{viewName}";
            return address;
        }

        public override bool Equals(System.Object obj)
        {
            if (obj == null)
                return false;

            if (this == obj)
                return true;

            if (this.GetType() != obj.GetType())
                return false;

            ActionResult test = (ActionResult)obj;

            if (this.routeUrl == test.routeUrl 
                && this.controllerName == test.controllerName 
                && this.viewName == test.viewName)
            {
                return true;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return routeUrl.GetHashCode(); 
        }

    }
}