using UnityEngine;
using UnityEditor;
using System.IO;

public class EnglishDeutchObjectExporter : MonoBehaviour
{
    [MenuItem("Tools/Export EnglishDeutchObject to JSON")]
    public static void ExportToJson()
    {
        // Ask user to select ScriptableObject asset
        EnglishDeutchObject data = Selection.activeObject as EnglishDeutchObject;

        if (data == null)
        {
            Debug.LogError("Please select an EnglishDeutchObject asset in the Project view first.");
            return;
        }

        // Convert to JSON (pretty print)
        string json = JsonUtility.ToJson(data, true);

        // Choose save location
        string path = EditorUtility.SaveFilePanel(
            "Save JSON file",
            "",
            data.name + ".json",
            "json"
        );

        if (string.IsNullOrEmpty(path))
        {
            Debug.Log("Export canceled.");
            return;
        }

        // Write JSON to file
        File.WriteAllText(path, json);

        Debug.Log("✅ JSON exported to: " + path);
    }
}
