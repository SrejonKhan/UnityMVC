using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace UnityMVC
{
    internal class LayoutLoader
    {
        private static AsyncOperationHandle handle;

        internal static IEnumerator Load(AssetReference layout, bool makeLayoutRoot)
        {
            if (layout == null)
                yield break;

            handle = layout.LoadAssetAsync<GameObject>();

            yield return handle;

            if (handle.Result == null)
                throw new System.ArgumentNullException("Result", $"Couldn't find view.");

            GameObject go = null;

            if (!MVC.Root)
                throw new System.ArgumentNullException("MVC Root", "Root can't be null");

            go = Object.Instantiate((GameObject)handle.Result, MVC.Root.transform);

            if (makeLayoutRoot)
                MVC.Root = go;

            Debug.Log(MVC.Root.gameObject.name);
        }

    }
}
