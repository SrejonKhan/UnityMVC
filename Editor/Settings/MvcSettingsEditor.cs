using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UnityMVC.Editor
{
    internal class MvcSettingsEditor
    {
        [SettingsProvider]
        internal static SettingsProvider CustomSettinsProvider()
        {
            var provider = new SettingsProvider("Project/UnityMVC", SettingsScope.Project)
            {
                guiHandler = context => UnityEditor.Editor.CreateEditor(MvcSettings.Instance).OnInspectorGUI(),
                keywords = SettingsProvider.GetSearchKeywordsFromSerializedObject(new SerializedObject(MvcSettings.Instance))
            };

            return provider;
        }
    }
}