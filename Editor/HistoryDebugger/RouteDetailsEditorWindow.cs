using UnityEditor;
using UnityEngine;

namespace UnityMVC.Editor
{
    public class RouteDetailsEditorWindow : EditorWindow
    {
        private GUIStyle titleLabelStyle;

        private ActionResult routeResult;
        private GameObject instantiatedObject;
        private object[] historyArgs;
        private Vector2 scrollPosition;

        private bool isInitialized = false;

        private void InitializeTitleLabelStyle()
        {
            titleLabelStyle = new GUIStyle(GUI.skin.label);
            titleLabelStyle.fontSize = 20;
            titleLabelStyle.fontStyle = FontStyle.Bold;
            titleLabelStyle.alignment = TextAnchor.MiddleCenter;
        }

        private void DrawTitleLabel(string title)
        {
            GUILayout.Label(title, titleLabelStyle, GUILayout.ExpandWidth(true), GUILayout.Height(30));
        }

        public void SetRouteDetails(ActionResult result, object[] argsList)
        {
            routeResult = result;
            instantiatedObject = result.InstantiatedObject;
            historyArgs = argsList;
        }

        private void OnGUI()
        {
            if (!isInitialized)
            {
                InitializeTitleLabelStyle();
                isInitialized = true;
                this.Repaint();
            }

            DrawTitleLabel("Route Details");

            EditorGUILayout.LabelField("Route URL:", routeResult.RouteUrl);
            EditorGUILayout.ObjectField("Instantiated Object:", instantiatedObject, typeof(GameObject), true);

            // History Args
            GUILayout.Label("History Args", EditorStyles.boldLabel);
            if (historyArgs != null && historyArgs.Length > 0)
            {
                scrollPosition = EditorGUILayout.BeginScrollView(
                    scrollPosition, 
                    GUILayout.ExpandWidth(true), 
                    GUILayout.ExpandHeight(true)
                    );

                for (int i = 0; i < historyArgs.Length; i++)
                {
                    EditorGUILayout.LabelField($"Args {i + 1}:", historyArgs[i].ToString());
                }
                EditorGUILayout.EndScrollView();
            }
            else
            {
                EditorGUILayout.LabelField("No history argument available.");
            }
        }
    }
}
