using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;

namespace UnityMVC.Editor
{
    public class ScaffolidingWindow : EditorWindow
    {
        //private string controllerName;
        //private string viewName;
        //private string modelName;

        private int selectedControllerIndex = 0;
        private int selectedActionIndex = 0;

        private List<Type> controllers = new List<Type>();
        List<MethodInfo> actions = new List<MethodInfo>();
        private string viewName;
        private Type viewType;

        private UnityEngine.Object viewPrefab;

        [MenuItem("Window/MVC/Scaffolding")]
        public static void ShowWindow()
        {
            EditorWindow.GetWindow<ScaffolidingWindow>("MVC Scaffolding", true);

        }

        private void OnGUI()
        {
            GUILayout.Label("Register View ", EditorStyles.boldLabel);

            GetControllers();

            if (controllers.Count > 0 && selectedControllerIndex != 0)
                GetActions();

            if (actions.Count > 0 && selectedActionIndex != 0)
                GetViewName();

            if (!string.IsNullOrEmpty(viewName) && actions.Count > 0 && selectedActionIndex != 0)
                viewType = GetViewType();

            if (viewType != null && selectedActionIndex != 0)
                viewPrefab = EditorGUILayout.ObjectField("View Prefab", viewPrefab, typeof(GameObject), false);

            if (viewPrefab && selectedActionIndex != 0 && selectedActionIndex != 0)
                HandlePrefabViewRegister();
        }

        private List<Type> GetControllers()
        {
            Assembly[] availableAssembly = AppDomain.CurrentDomain.GetAssemblies();

            foreach (Assembly assembly in availableAssembly)
                controllers.AddRange(EditorReflection.GetMonoControllerClasses(assembly));

            List<string> controllerNames = controllers.Select(t => t.Name).ToList();
            controllerNames.Insert(0, "None");
            selectedControllerIndex = EditorGUILayout.Popup("Controller", selectedControllerIndex, controllerNames.ToArray());

            return controllers;
        }

        private List<MethodInfo> GetActions()
        {
            var (validControllerSelection, controllerIndex) = ValidateControllerSelection();
            if (!validControllerSelection)
                return null;

            Type selectedController = controllers[controllerIndex];

            var flags = (BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
            actions.AddRange(selectedController.GetMethods(flags).Where(m => m.ReturnType == typeof(ViewResult)));

            List<string> actionNames = actions.Select(t => t.Name).ToList();
            actionNames.Insert(0, "None");
            selectedActionIndex = EditorGUILayout.Popup("Actions", selectedActionIndex, actionNames.ToArray());

            return actions;
        }

        private string GetViewName()
        {
            var (validControllerSelection, controllerIndex) = ValidateControllerSelection();
            if (!validControllerSelection)
                return "";

            Type selectedController = controllers[controllerIndex];

            int actionIndex = selectedActionIndex - 1;
            if (actionIndex < 0)
                return "";

            MethodInfo selectedAction = actions[actionIndex];

            viewName = $"{selectedController.Name.Replace("Controller", "")}{selectedAction.Name}";

            ReadOnlyTextField("View", viewName);

            return viewName;
        }

        private Type GetViewType()
        {
            if (string.IsNullOrEmpty(viewName))
                return null;

            Type view = null;
            string fullViewName = $"{viewName}View";

            Assembly[] availableAssembly = AppDomain.CurrentDomain.GetAssemblies();

            foreach (Assembly assembly in availableAssembly)
            {
                view = EditorReflection.GetViewClass(assembly, fullViewName); // last arg -> HomeIndexView.cs
                if (view != null)
                    break;
            }
            var viewObj = AssetDatabase.LoadAssetAtPath($"{MvcSettings.Instance.viewClassFolder}/{fullViewName}.cs", typeof(MonoScript));
            if (viewObj != null)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("View Class", GUILayout.Width(EditorGUIUtility.labelWidth));
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.ObjectField(viewObj, typeof(MonoScript), false);
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.EndHorizontal();
            }
            else if (view != null)
                ReadOnlyTextField("View Class", $"{fullViewName}.cs found in Assembly, but not in {MvcSettings.Instance.viewClassFolder}.");

            return view;
        }

        private bool IsCorrectAddress(string expected, string actual)
        {
            if (expected == actual)
                return true;
            return false;
        }

        private string MapViewPrefabAddress()
        {
            var (validControllerSelection, controllerIndex) = ValidateControllerSelection();
            if (!validControllerSelection)
                return "";

            var (validActionSelection, actionIndex) = ValidateActionSelection();
            if (!validActionSelection)
                return "";

            string selectedControllerName = controllers[controllerIndex].Name.
                Replace("Controller", "");

            string selectedActionName = actions[actionIndex].Name;

            string address = $"{selectedControllerName}/{selectedActionName}";

            return address;
        }

        private (bool, int) ValidateControllerSelection()
        {
            int controllerIndex = selectedControllerIndex - 1;
            if (controllerIndex < 0)
                return (false, -1);

            return (true, controllerIndex);
        }

        private (bool, int) ValidateActionSelection()
        {
            int actionIndex = selectedActionIndex - 1;
            if (actionIndex < 0)
                return (false, -1);

            return (true, actionIndex);
        }

        private void ReadOnlyTextField(string label, string text)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(label, GUILayout.Width(EditorGUIUtility.labelWidth));
            EditorGUILayout.SelectableLabel(text, EditorStyles.textField, GUILayout.Height(EditorGUIUtility.singleLineHeight));
            EditorGUILayout.EndHorizontal();
        }

        private void HandlePrefabViewRegister()
        {
            string expectedAddress = MapViewPrefabAddress();

            // check in addressable if view prefeb is registered correctly
            var group = AddressableEditor.GetOrCreateGroup(MvcSettings.Instance.groupName);

            var (hasViewEntry, viewEntry) = AddressableEditor.CheckAssetEntry(viewPrefab, group); // mvc group

            var (hasViewEntryOther, viewEntryOther) = AddressableEditor.ChechAssetEntryInAllGroups(viewPrefab); // all group

            // exist in different group, not mvc (incorrect group)
            if (hasViewEntryOther && !hasViewEntry)
            {
                EditorGUILayout.HelpBox($"View Prefab is Registered in different group - {viewEntryOther.address}. " +
                $"Correct address for this selection is {expectedAddress} in {MvcSettings.Instance.groupName} Group.",
                MessageType.Error);

                //fix Group
                if (GUILayout.Button("Fix Group"))
                    AddressableEditor.ChangeEntryGroup(viewEntryOther, group);
            }

            // correct group
            if (hasViewEntry)
            {
                bool isCorrectAddress = IsCorrectAddress(expectedAddress, viewEntry.address);

                // wrong address or allocation
                if (!isCorrectAddress)
                {
                    EditorGUILayout.HelpBox($"View Prefab is already Registered, but in different address - {viewEntry.address}. " +
                        $"Correct address for this selection is {expectedAddress}.",
                        MessageType.Warning);

                    //fix address
                    if (GUILayout.Button("Fix Address"))
                        AddressableEditor.ChangeEntryAddress(viewEntry, expectedAddress);
                }
                // correct address and group
                else
                {
                    EditorGUILayout.HelpBox($"View Prefab is already Registered in correct address - {viewEntry.address}.",
                    MessageType.Info);
                }
            }
            // entry not found anywhere, register
            else if (!hasViewEntryOther)
            {
                // create a entry
                if (GUILayout.Button("Register"))
                    AddressableEditor.CreateEntry(viewPrefab, group, expectedAddress);
            }
        }
    }
}