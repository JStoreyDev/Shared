using System;
using UnityEditor;
using UnityEngine;

namespace JS
{
    public class PackageTemplateDialog : EditorWindow
    {
        string search;
        public static string Show(string windowName, string search)
        {
            var dialog = CreateInstance<PackageTemplateDialog>();
            dialog.titleContent = new GUIContent(windowName);
            dialog.maxSize = new Vector2(500, 500);
            dialog.minSize = new Vector2(500, 500);
            dialog.position = new Rect(Screen.width / 2f, Screen.height / 2f, 300, 100);
         
            dialog.search = search;
            dialog.ShowModalUtility();
            return dialog.search;
        }
        
        Vector2 _position;
        void OnGUI()
        {
            GUILayout.Space(25);
            _userSearch = EditorGUILayout.TextField("Search", _userSearch);
            var results = AssetsFolder.Search(_userSearch);
            _position = EditorGUILayout.BeginScrollView(_position);
            foreach (var asset in results) DrawAsset(asset);
            EditorGUILayout.EndScrollView();
            
        }

        void DrawAsset(AssetInfo asset)
        {
            GUILayout.BeginHorizontal("groupbox");
   
            if (GUILayout.Button(_icon, GUILayout.Width(50), GUILayout.Height(50)))
            {
                AssetsFolder.AddToProject(asset);
                Close();
                
            }
            GUILayout.BeginVertical();
            GUILayout.Label(asset.Name);
            GUILayout.Label(asset.Publisher);
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            
        }

        GUIContent _icon;
        void OnEnable()
        {
            _icon = EditorGUIUtility.IconContent("Download-Available@2x");
            
        }

        string _userSearch;
    }
}