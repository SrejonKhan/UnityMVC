using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityMVC.Utils
{
    public static class GameObjectExtension
    {
        // credit - https://stackoverflow.com/a/62553770/14685101
        public static T AddComponent<T>(this GameObject go, T toAdd) where T : Component
        {
            return go.AddComponent<T>().GetCopyOf(toAdd) as T;
        }
    }
}
