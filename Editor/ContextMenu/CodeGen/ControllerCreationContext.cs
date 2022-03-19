using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UnityMVC.Editor
{
    public static class ControllerCreationContext
    {
        [MenuItem("Assets/MVC/Create Controller Class")]
        private static void CreateControllerClass()
        {
            string filePath;
            if (Selection.assetGUIDs.Length == 0)
                filePath = "Assets/";
            else
                filePath = AssetDatabase.GUIDToAssetPath(Selection.assetGUIDs[0]);

            var window = EditorWindow.GetWindow<CodeGenWindow>("Code Gen", true);

            window.filePath = filePath;
            window.self = window;
        }
    }
}
