using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace UnityMVC
{
    public class MvcInitializer : MonoBehaviour
    {
        public GameObject root;

        [HideInInspector]
        public AssetReference layout;
        [HideInInspector]
        public bool makeLayoutRoot = true; 

        private void Awake()
        {
            MVC.Init(gameObject, root, layout, makeLayoutRoot);
        }
    }
}
