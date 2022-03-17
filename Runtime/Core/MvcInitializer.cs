using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityMVC
{
    public class MvcInitializer : MonoBehaviour
    {
        private void Awake()
        {
            MVC.Init(gameObject);
        }
    }
}
