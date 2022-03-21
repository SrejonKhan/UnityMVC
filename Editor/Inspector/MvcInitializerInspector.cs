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
        private int selectedLayout = 0;

        public override void OnInspectorGUI()
        {
            mvcInitializer = mvcInitializer ?? (MvcInitializer)target;

            base.OnInspectorGUI();

            var entries = AddressableEditor.GetAllEntriesInAllGroups();
            var availableLayout = entries.Where(e => e.address.Split('/')[0] == "Layout").ToArray();
            var availableLayoutAddresses = availableLayout.Select(l => l.address.Split('/')[1]).ToList();
            availableLayoutAddresses.Insert(0, "None");

            selectedLayout = EditorGUILayout.Popup("Layout", selectedLayout, availableLayoutAddresses.ToArray());

            mvcInitializer.layout = (selectedLayout == 0)
                ? null 
                : new AssetReference(availableLayout[selectedLayout - 1].guid);

            if(mvcInitializer.layout != null)
                mvcInitializer.makeLayoutRoot = EditorGUILayout.Toggle("Make Layout Root of Views?", mvcInitializer.makeLayoutRoot);
        }
    }
}