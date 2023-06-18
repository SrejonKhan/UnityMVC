using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace UnityMVC.Editor
{
    public class RouteHistoryEditorWindow : EditorWindow
    {
        private GUIStyle titleLabelStyle;
        private GUIStyle subtitleLabelStyle;

        private List<ActionResult> history;
        private Dictionary<string, object[]> historyArgs;
        private int currentIndex;
        private Vector2 scrollPosition;

        private bool isInitialized = false;

        [MenuItem("Window/MVC/History Debugger/Route History")]
        public static void OpenWindow()
        {
            RouteHistoryEditorWindow window = GetWindow<RouteHistoryEditorWindow>();
            window.titleContent = new GUIContent("Route History Debugger");
            window.Show();
        }

        private void OnEnable()
        {
            EditorApplication.update += UpdateHistoryIndex;
            history = MVC.DANGEROUS_GetHistoryList();
            historyArgs = MVC.DANGEROUS_GetHistoryArgs();
        }

        private void OnDisable()
        {
            EditorApplication.update -= UpdateHistoryIndex;
        }

        private void UpdateHistoryIndex()
        {
            currentIndex = MVC.GetCurrentHistoryIndex();
            Repaint();
        }

        private void InitializeTitleLabelStyle()
        {
            titleLabelStyle = new GUIStyle(GUI.skin.label);
            titleLabelStyle.fontSize = 22;
            titleLabelStyle.fontStyle = FontStyle.Bold;
            titleLabelStyle.alignment = TextAnchor.MiddleCenter;
        }

        private void InitializeSubtitleLabelStyle()
        {
            subtitleLabelStyle = new GUIStyle(GUI.skin.label);
            subtitleLabelStyle.fontSize = 14;
            subtitleLabelStyle.fontStyle = FontStyle.Bold;
            subtitleLabelStyle.alignment = TextAnchor.MiddleCenter;
        }

        private void DrawTitleLabel(string title)
        {
            GUILayout.Label(title, titleLabelStyle, GUILayout.ExpandWidth(true), GUILayout.Height(30));
        }

        private void DrawSubtitleLabel(string subtitle)
        {
            GUILayout.Label(subtitle, subtitleLabelStyle, GUILayout.ExpandWidth(true));
        }

        private void OnGUI()
        {
            if (!isInitialized)
            {
                InitializeTitleLabelStyle();
                InitializeSubtitleLabelStyle();
                isInitialized = true;
                this.Repaint();
            }
            
            DrawTitleLabel("History Debugger");
            DrawSubtitleLabel("Route History");

            GUILayout.Label($"Current History Index = {currentIndex}");

            // Display the history stack as a scrollable table
            scrollPosition = EditorGUILayout.BeginScrollView(
                scrollPosition, 
                GUILayout.ExpandWidth(true), 
                GUILayout.ExpandHeight(true)
                );

            GUILayout.BeginVertical("box");

            // table header
            GUILayout.BeginHorizontal();
            GUILayout.Label("Index", EditorStyles.boldLabel, GUILayout.Width(50));
            GUILayout.Label("Route URL", EditorStyles.boldLabel, GUILayout.ExpandWidth(true));
            GUILayout.Label("Args Count", EditorStyles.boldLabel, GUILayout.Width(80));
            GUILayout.Label("Action", EditorStyles.boldLabel, GUILayout.Width(60));
            GUILayout.EndHorizontal();

            for (int i = history.Count - 1; i >= 0; i--)
            {
                bool isCurrent = (i == currentIndex);
                GUIStyle labelStyle = isCurrent ? GetHighlightedLabelStyle() : EditorStyles.label;

                GUILayout.BeginHorizontal();

                // index 
                GUILayout.Label(i.ToString(), labelStyle, GUILayout.Width(50));
                // route url
                GUILayout.Label(history[i].RouteUrl, labelStyle, GUILayout.ExpandWidth(true));
                // arguments
                GUILayout.Label(
                    historyArgs.ContainsKey(history[i].RouteUrl) 
                    ? historyArgs[history[i].RouteUrl].Length.ToString() 
                    : "0", 
                    labelStyle, GUILayout.Width(80)
                    );

                // details button
                if (GUILayout.Button("Details", GUILayout.Width(60)))
                    OpenRouteDetailsEditorWindow(history[i]);

                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
        }

        private GUIStyle GetHighlightedLabelStyle()
        {
            GUIStyle style = new GUIStyle(EditorStyles.boldLabel);
            style.normal.textColor = Color.yellow;
            return style;
        }

        private void OpenRouteDetailsEditorWindow(ActionResult result)
        {
            RouteDetailsEditorWindow window = CreateInstance<RouteDetailsEditorWindow>();
            window.titleContent = new GUIContent("Route Details");
            window.SetRouteDetails(result, 
                historyArgs.ContainsKey(result.RouteUrl) 
                ? historyArgs[result.RouteUrl] 
                : null
                );
            window.Show();
        }
    }
}
