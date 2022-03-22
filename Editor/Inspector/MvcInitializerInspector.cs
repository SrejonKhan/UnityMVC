using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using UnityEngine.AddressableAssets;

namespace UnityMVC.Editor
{
    [CustomEditor(typeof(MvcInitializer))]
    public class MvcInitializerInspector : UnityEditor.Editor
    {
        private MvcInitializer mvcInitializer;

        public override void OnInspectorGUI()
        {
            mvcInitializer = mvcInitializer ?? (MvcInitializer)target;

            base.OnInspectorGUI();

            if (mvcInitializer.layout != null)
            {
                mvcInitializer.makeLayoutRoot = EditorGUILayout.Toggle("Make Layout Root of Views?", mvcInitializer.makeLayoutRoot);
            }
        }
    }
}