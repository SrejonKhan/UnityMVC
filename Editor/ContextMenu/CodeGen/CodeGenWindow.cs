using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace UnityMVC.Editor
{
    public class CodeGenWindow : EditorWindow
    {
        internal string filePath;

        private string controllerName;

        internal CodeGenWindow self;

        private void OnGUI()
        {
            if (string.IsNullOrEmpty(filePath))
            {
                EditorGUILayout.HelpBox("No filepath provided from Context Menu, maybe a bug.", MessageType.Error);
                return;
            }

            EditorGUILayout.LabelField("Create Controller Class", EditorStyles.boldLabel);
            controllerName = EditorGUILayout.TextField("Controller Name", controllerName);

            if (!GUILayout.Button("Create Controller Class")) 
                return;

            string formalControllerName = controllerName + "Controller";

            string code =
@"// This is generated code by UnityMVC Scaffolder
// This is a Controller Class, which derives from MonoController
// MonoController derives from MonoBehaviour
// It is responsible for interaction between View and Model, 
// also User Action (Calling Navigate from MVC)
using UnityMVC;

public class CLASS_NAME : MonoController
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // This is an Action Method
    // calling it from MVC will instantiate 
    // CONTROLLER_VIEW_NAME View 
    public ViewResult Index()
    {
        return View();
    }
}

";
            // idk, why format is not working here? missing something i believe!
            code = code.Replace("CLASS_NAME", formalControllerName);
            code = code.Replace("CONTROLLER_VIEW_NAME", controllerName + "Index");

            string refinedPath = Path.Combine(filePath, formalControllerName + ".cs");
            File.WriteAllText(refinedPath, code);
            AssetDatabase.ImportAsset(refinedPath);

            // highlight
            EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<Object>(refinedPath));

            Debug.Log($"{formalControllerName} is created at {refinedPath}");
            
            // close window
            self?.Close();

        }
    }
}
