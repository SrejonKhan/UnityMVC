using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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
        private int selectedControllerIndex = 0;
        private int lastSelectedControllerIndex = 0;
        private int selectedActionIndex = 0;
        private int lastSelectedActionIndex = 0;


        private List<Type> controllers = new List<Type>();
        List<MethodInfo> actions = new List<MethodInfo>();
        private string viewName;
        private Type viewType;

        private UnityEngine.Object viewPrefab;

        private UnityEngine.Object layoutPrefab;
        private string layoutName;

        [MenuItem("Window/MVC/Scaffolding")]
        public static void ShowWindow()
        {
            EditorWindow.GetWindow<ScaffolidingWindow>("MVC Scaffolding", true);

        }

        private void OnGUI()
        {
            GUILayout.Label("Register View ", EditorStyles.boldLabel);

            // Get all MonoControllers from project
            GetControllers();

            // Get all Action method from selected MonoController
            GetActions();

            // Get view name by combining Controller and Action name
            GetViewName();

            // Get View Class of View Name, if it;s avaialble.
            viewType = GetViewType(); // get view type

            // create View class, if not available
            // Ask for boilerplate code generation, if not available
            if (viewType == null && selectedControllerIndex != 0 && selectedActionIndex != 0)
                CreateNewType();

            // clear view prefab if new options are selected
            ClearViewPrefabSelection();

            // search view prefab in addressable entry
            GetViewPrefabFromAddressable();

            // Select view prefeb
            if (viewType != null && selectedControllerIndex != 0 && selectedActionIndex != 0)
                viewPrefab = EditorGUILayout.ObjectField("View Prefab", viewPrefab, typeof(GameObject), false);

            // Register or Modify prefab entry in Addressable
            if (viewPrefab && selectedControllerIndex != 0 && selectedActionIndex != 0)
                HandlePrefabViewRegister();

            // divider
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        }

        private void GetControllers()
        {
            Assembly[] availableAssembly = AppDomain.CurrentDomain.GetAssemblies();

            foreach (Assembly assembly in availableAssembly)
                controllers.AddRange(EditorReflection.GetMonoControllerClasses(assembly));

            List<string> controllerNames = controllers.Select(t => t.Name).ToList();
            controllerNames.Insert(0, "None");

            selectedControllerIndex = EditorGUILayout.Popup("Controller", selectedControllerIndex, controllerNames.ToArray());
        }

        private void GetActions()
        {
            var (validControllerSelection, controllerIndex) = ValidateControllerSelection();
            if (!validControllerSelection)
                return;

            //if (controllers.Count > 0 && selectedControllerIndex != 0)
            //    return;

            Type selectedController = controllers[controllerIndex];

            var flags = (BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
            actions.AddRange(selectedController.GetMethods(flags).Where(m => m.ReturnType == typeof(ViewResult)));

            List<string> actionNames = actions.Select(t => t.Name).ToList();
            actionNames.Insert(0, "None");
            selectedActionIndex = EditorGUILayout.Popup("Actions", selectedActionIndex, actionNames.ToArray());
        }

        private void GetViewName()
        {
            var (validControllerSelection, controllerIndex) = ValidateControllerSelection();
            if (!validControllerSelection)
                return;

            //if (actions.Count > 0 && selectedActionIndex != 0)
            //    return;

            Type selectedController = controllers[controllerIndex];

            int actionIndex = selectedActionIndex - 1;
            if (actionIndex < 0)
                return;

            MethodInfo selectedAction = actions[actionIndex];

            viewName = $"{selectedController.Name.Replace("Controller", "")}{selectedAction.Name}";

            ReadOnlyTextField("View", viewName);
        }

        private void ClearViewPrefabSelection()
        {
            bool controllerSelectionChanged = lastSelectedControllerIndex != selectedControllerIndex;

            if (controllerSelectionChanged)
                lastSelectedControllerIndex = selectedControllerIndex;

            bool actionSelectionChanged = lastSelectedActionIndex != selectedActionIndex;

            if (actionSelectionChanged)
                lastSelectedActionIndex = selectedActionIndex;

            if (controllerSelectionChanged || actionSelectionChanged)
                viewPrefab = null;

        }

        private void GetViewPrefabFromAddressable()
        {
            // sanity checks
            var (validControllerSelection, controllerIndex) = ValidateControllerSelection();
            var (validActionSelection, actionIndex) = ValidateActionSelection();

            if (!validControllerSelection || !validActionSelection)
                return;

            Type selectedController = controllers[controllerIndex];
            MethodInfo selectedAction = actions[actionIndex];

            string address = $"{selectedController.Name.Replace("Controller", "")}/{selectedAction.Name}";

            var entry = AddressableEditor.FindEntryByAddress(address);

            if (entry == null)
                return;

            viewPrefab = AssetDatabase.LoadAssetAtPath(entry.AssetPath, typeof(GameObject));
        }

        private Type GetViewType()
        {
            if (string.IsNullOrEmpty(viewName))
                return null;

            // sanity checks
            var (validControllerSelection, _) = ValidateControllerSelection();
            var (validActionSelection, _) = ValidateActionSelection();

            if (!validControllerSelection || !validActionSelection)
                return null;

            //if (actions.Count > 0 && selectedControllerIndex != 0 && selectedActionIndex != 0)
            //    return null;

            Type viewType = null;
            string fullViewName = $"{viewName}View";

            Assembly[] availableAssembly = AppDomain.CurrentDomain.GetAssemblies();

            foreach (Assembly assembly in availableAssembly)
            {
                viewType = EditorReflection.GetViewClass(assembly, fullViewName); // last arg -> HomeIndexView.cs
                if (viewType != null)
                    break;
            }

            if (viewType == null) return null;

            string viewTypeAssetPath = "";

            var matchScriptsGUID = AssetDatabase.FindAssets($"{fullViewName} t:script");
            foreach (var scriptGUID in matchScriptsGUID)
            {
                string scriptPath = AssetDatabase.GUIDToAssetPath(scriptGUID);
                var script = (MonoScript)AssetDatabase.LoadAssetAtPath(scriptPath, typeof(MonoScript));
                var scriptType = script.GetClass();

                if (scriptType == viewType)
                {
                    if (scriptType.Namespace != viewType.Namespace)
                        continue;
                    if (scriptType.Assembly != viewType.Assembly)
                        continue;
                    viewTypeAssetPath = scriptPath;
                    break;
                }
            }

            if (string.IsNullOrEmpty(viewTypeAssetPath)) return null;

            var viewObj = AssetDatabase.LoadAssetAtPath(viewTypeAssetPath, typeof(MonoScript));
            if (viewObj != null)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("View Class", GUILayout.Width(EditorGUIUtility.labelWidth));
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.ObjectField(viewObj, typeof(MonoScript), false);
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.EndHorizontal();
            }

            return viewType;
        }

        private Type CreateNewType()
        {
            string viewClassName = $"{viewName}View.cs";
            string viewClassNameWithoutExt = viewClassName.Replace(".cs", "");

            EditorGUILayout.HelpBox($"{viewClassName} is not found. Create new derived class from ViewContainer and name as {viewClassName}.", MessageType.Error);

            if (!GUILayout.Button("Create View Class")) return null;

            string path = EditorUtility.SaveFilePanelInProject("Save View Class", viewClassName, "cs",
                "Please enter a file name to save the view class to");

            if (string.IsNullOrEmpty(path)) return null;

            string code =
@"// This is generated code by UnityMVC Scaffolder
// This can be added to any gameobject as Component
// ViewContainer derives from MonoBehaviour
// ViewContainer provides some property

using UnityMVC;

public class CLASS_NAME : ViewContainer
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
";
            code = code.Replace("CLASS_NAME", viewClassNameWithoutExt);

            File.WriteAllText(path, code);
            AssetDatabase.ImportAsset(path);


            Type view = null;

            Assembly[] availableAssembly = AppDomain.CurrentDomain.GetAssemblies();

            foreach (Assembly assembly in availableAssembly)
            {
                view = EditorReflection.GetViewClass(assembly, viewClassNameWithoutExt); // last arg -> HomeIndexView.cs
                if (view != null)
                    break;
            }

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

                    // remove entry 
                    if (GUILayout.Button("Remove Entry"))
                        AddressableEditor.RemoveEntry(viewEntry);
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