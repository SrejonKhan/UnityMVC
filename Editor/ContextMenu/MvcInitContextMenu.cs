using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;

namespace UnityMVC.Editor
{
    public static class MvcInitContextMenu
    {
        [MenuItem("GameObject/MVC/Create Mvc Initializer", false, +100)]
        public static void InstantiateMvcInit()
        {
            // check if we already have MvcInit in Scene? 
            var existingInitializer = Object.FindObjectOfType<MvcInitializer>(false);
            if (existingInitializer)
            {
                Debug.Log("MvcInitializer already exist in the scene.");
                EditorGUIUtility.PingObject(existingInitializer);
                return;
            }

            var go = new GameObject("MvcContainer");
            var initializer = go.AddComponent<MvcInitializer>();

            // assign canvas
            var canvas = Object.FindObjectOfType<Canvas>(true);
            
            if (canvas != null)
                initializer.root = canvas.gameObject;

            Undo.RegisterCreatedObjectUndo(go, "Created MvcInitializer");
        }
    }
}