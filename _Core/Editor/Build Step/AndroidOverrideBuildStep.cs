using OGT;
using UnityEditor;
using UnityEditor.Build.Profile;
using UnityEngine;

public class AndroidOverrideBuildStep : PreBuildStep
{
    [SerializeField] private bool splitApplicationBinary;
    [SerializeField] private string keyaliasPass;
    [SerializeField] private string keystorePass;

    public override string Name => "Android Override Build Step";

    public override void Run(BuildProfile buildProfile)
    {
        PlayerSettings.Android.splitApplicationBinary = this.splitApplicationBinary;
        PlayerSettings.Android.keyaliasPass = this.keyaliasPass;
        PlayerSettings.Android.keystorePass = this.keystorePass;
    }
}
