using UnityEngine;
using UnityEditor;
using System.IO;

namespace Op3nPinball.DopeSheet
{
    public class LEDDefinitionImport : EditorWindow
    {
        public string jsonPath = "";
        GameObject parserGameObject;

        [MenuItem("Op3nPinball/LED Definition Import")]
        public static void ShowWindow()
        {
            LEDDefinitionImport window = (LEDDefinitionImport)EditorWindow.GetWindow(typeof(LEDDefinitionImport));
            window.Show();
        }

        void OnGUI()
        {
            GUILayout.Label("Select JSON file:");
            if (GUILayout.Button("Select File"))
            {
                jsonPath = EditorUtility.OpenFilePanel("Select JSON file", "", "json");
            }
            GUILayout.Label(jsonPath);

            if (GUILayout.Button("Create Objects from JSON"))
            {
                if (!string.IsNullOrEmpty(jsonPath) && File.Exists(jsonPath))
                {
                    parserGameObject = new GameObject();
                    ModuleLEDConfigParser parser = parserGameObject.AddComponent<ModuleLEDConfigParser>();
                    parser.ParseJSON(jsonPath);
                    parser.CreateObjectsFromJSON(Path.GetFileNameWithoutExtension(jsonPath));
                    DestroyImmediate(parserGameObject);
                }
                else
                {
                    Debug.LogError("JSON file not found at path: " + jsonPath);
                }
            }
        }
    }
}