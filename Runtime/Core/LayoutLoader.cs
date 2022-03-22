using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace UnityMVC
{
    internal class LayoutLoader
    {
        /// <summary>
        /// Load Layout of MVC
        /// </summary>
        /// <param name="layout"></param>
        /// <param name="root"></param>
        /// <param name="makeLayoutRoot"></param>
        /// <exception cref="System.ArgumentNullException"></exception>
        internal static void Load(GameObject layout, Transform root, bool makeLayoutRoot, UnityEvent<GameObject> onInstantiate)
        {
            if (layout == null)
                return;

            GameObject go = null;

            if (!root)
                throw new System.ArgumentNullException("Root", "Root can't be null");

            go = Object.Instantiate(layout, root);

            if (makeLayoutRoot)
                MVC.Root = go;

            onInstantiate?.Invoke(go);
        }
    }
}
