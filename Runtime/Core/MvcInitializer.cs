using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityMVC
{
    public class MvcInitializer : MonoBehaviour
    {
        public Canvas rootCanvas;

        private void Awake()
        {
            MVC.Init(gameObject, rootCanvas);
        }
    }
}
