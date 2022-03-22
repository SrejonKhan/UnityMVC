using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;

namespace UnityMVC
{
    public class MvcInitializer : MonoBehaviour
    {
        public GameObject root;

        public GameObject layout;
        [HideInInspector]
        public bool makeLayoutRoot = true;
        [HideInInspector]
        public UnityEvent<GameObject> onLayoutInstantiated; 


        private void Awake()
        {
            MVC.Init(gameObject, root, layout, makeLayoutRoot, onLayoutInstantiated);
        }
    }
}
