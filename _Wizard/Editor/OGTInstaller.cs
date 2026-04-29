#if UNITY_EDITOR

using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;

[InitializeOnLoad]
public class OGTInstaller : EditorWindow
{
    static OGTInstaller()
    {
        var manifestPath = "./Packages/manifest.json";
        var manifestJson = System.IO.File.ReadAllText(manifestPath);

        if (manifestJson.Contains("OGT OpenUPM") == false)
        {
            EditorApplication.delayCall += ShowInstallerWindow;
        }
    }

    [MenuItem("Tools/OGT Installer")]
    private static void ShowInstallerWindow()
    {
        var window = GetWindow<OGTInstaller>();
        window.titleContent = new GUIContent("OGT Installer");
        window.Show();
    }

    private void OnGUI()
    {
        GUILayout.Label("The OpenUPM registry is not added to your project.", EditorStyles.boldLabel);
        GUILayout.Label("Would you like to add the OpenUPM registry to your project?", EditorStyles.wordWrappedLabel);
        if (GUILayout.Button("Add OpenUPM Registry"))
        {
            AddOpenUpmRegistry();
            Client.Resolve();
            Close();
        }
    }

    private static void AddOpenUpmRegistry()
    {
        var addScopedRegistry = typeof(Client).GetMethod(
            "AddScopedRegistry",
            BindingFlags.Static | BindingFlags.NonPublic,
            null,
            new[] { typeof(string), typeof(string), typeof(string[]), typeof(bool) },
            null);

        if (addScopedRegistry == null)
        {
            throw new Exception("Could not get AddScopedRegistry method.");
        }

        addScopedRegistry.Invoke(null, new object[]
        {
            "OGT OpenUPM",
            "https://package.openupm.com",
            new[]
            {
                "com.lostsignal.ogt",
                "com.revenantx.litenetlib",
                "net.bunnycdn.storage",
                "com.mischief.markdownviewer",
            },
            false,
        });

        Debug.Log("OpenUPM registry added via reflection.");
    }
}

#endif
