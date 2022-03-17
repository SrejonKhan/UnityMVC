using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace UnityMVC
{
    public class MvcSettings : ScriptableObject
    {
        public string groupName = "UnityMVC";
        public string modelFolder;
        public string viewClassFolder = "Assets/Scripts/MVC/Views";
        public string controllerFolder;

        private static MvcSettings instance;
        public static MvcSettings Instance 
        {
            get 
            {
                LoadSettings();
                return instance; 
            }
        }

        private static void LoadSettings()
        {
            if (instance) return;

            instance = FindOrCreateInstance();
        }

        private static MvcSettings FindOrCreateInstance()
        {
            MvcSettings tempInstance;
            tempInstance = Resources.Load<MvcSettings>("MvcSettings");
            tempInstance = tempInstance ? tempInstance : Resources.LoadAll<MvcSettings>("").FirstOrDefault();
            tempInstance = tempInstance ? tempInstance : CreateAndSave();
            if (tempInstance == null) throw new System.Exception("Could not create or find setting for Firebase Rest Client.");
            return tempInstance;
        }

        static MvcSettings CreateAndSave()
        {
            var tempInstance = CreateInstance<MvcSettings>();

#if UNITY_EDITOR
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorApplication.delayCall = () => SaveAsset(tempInstance);
            }
            else
            {
                SaveAsset(tempInstance);
            }
#endif
            return tempInstance;
        }

        static void SaveAsset(MvcSettings instance)
        {
            string dir = "Assets/Resources";
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
#if UNITY_EDITOR
            AssetDatabase.CreateAsset(instance, "Assets/Resources/MvcSettings.asset");
            AssetDatabase.SaveAssets();
#endif
        }
    }
}