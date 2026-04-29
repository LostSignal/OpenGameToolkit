using UnityEditor;
using UnityEngine;

public class StringPromptWindow : EditorWindow
{
    private string input = "";
    private System.Action<string> onSubmit;

    public static void Show(string title, string message, System.Action<string> onSubmit)
    {
        var window = CreateInstance<StringPromptWindow>();
        window.titleContent = new GUIContent(title);
        window.onSubmit = onSubmit;
        window.ShowUtility();
    }

    private void OnGUI()
    {
        GUILayout.Label("Enter a value:", EditorStyles.boldLabel);
        input = EditorGUILayout.TextField(input);

        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("OK"))
        {
            onSubmit?.Invoke(input);
            Close();
        }

        if (GUILayout.Button("Cancel"))
            Close();

        GUILayout.EndHorizontal();
    }
}
