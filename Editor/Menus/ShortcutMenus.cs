using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;

public static class ShortcutMenus
{
    [Shortcut("Tools/Shortcut/Open in Explorer", KeyCode.E, ShortcutModifiers.Alt)]
    [MenuItem("Tools/Shortcut/Open in Explorer")]
    public static void OpenInExplorer()
    {
        if(!Selection.activeObject) return;
        var path = AssetDatabase.GetAssetPath(Selection.activeObject);
        if(string.IsNullOrWhiteSpace(path)) return;
        EditorUtility.RevealInFinder(path);
    }
   
    [MenuItem("Tools/Shortcut/Open in Explorer", true)]
    public static bool OpenInExplorerValidate() => Selection.activeObject != null;
    
    [MenuItem("Tools/Shortcut/Save Directory")]
    public static void OpenSaveFolder() => 
        Application.OpenURL("file://" + Application.persistentDataPath );

    [MenuItem("Tools/Shortcut/Settings/Player")]
    public static void OpenPlayerSettings() => SettingsService.OpenProjectSettings("Project/Player");

    [MenuItem("Tools/Shortcut/Settings/Editor")]
    public static void OpenEditorSettings() => SettingsService.OpenProjectSettings("Project/Editor");
    
    
    [MenuItem("Tools/Websites/Timezone Converter")]
    public static void Timezones() => 
        Application.OpenURL("https://everytimezone.com");
    
    
}