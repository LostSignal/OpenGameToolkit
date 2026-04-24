//-----------------------------------------------------------------------
// <copyright file="ProjectSettingsGeneral.cs" company="Lost Signal">
//     Copyright (c) Lost Signal. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using UnityEditor;
    using UnityEngine;

    public class ProjectSettingsGeneral : ProjectSettingsBase<ProjectSettingsGeneral>
    {
        private static readonly OGTLogger Logger = OGTLogger.OGTEditor;

#pragma warning disable 0649
        // Generated Output Directory
        [SerializeField] private string generatedOutputDirectory;

        // Line Endings
        [SerializeField] private bool forceProjectLineEndings;
        [SerializeField] private LineEndings projectLineEndings;

        // Parallel Import
        [SerializeField] private bool forceParallelImport;
        [SerializeField] private int desiredImportWorkerCount;
        [SerializeField] private int standbyImportWorkerCount;
        [SerializeField] private int idleImportWorkerShutdownDelayInSeconds;

        // Serialization
        [SerializeField] private bool forceSerializationMode;
        [SerializeField] private SerializationMode serializationMode;
#pragma warning restore 0649

        public enum LineEndings
        {
            Unix,
            Windows,
        }

        public string GeneratedOutputDirectory => this.generatedOutputDirectory;

        public override string AssetName => nameof(ProjectSettingsGeneral);

        public override void LoadDefaults()
        {
            // ---- Generated Output Directory ----
            this.generatedOutputDirectory = "Assets/Third Party/OGT";

            // Line Endings
            this.forceProjectLineEndings = true;
            this.projectLineEndings = LineEndings.Unix;

            // Parallel Import
            this.forceParallelImport = true;
            this.desiredImportWorkerCount = 12;
            this.standbyImportWorkerCount = 4;
            this.idleImportWorkerShutdownDelayInSeconds = 60;

            // Serialization
            this.forceSerializationMode = true;
            this.serializationMode = SerializationMode.ForceText;
        }

        public override void Initialize()
        {
            base.Initialize();
            this.ApplyAllSettings();
        }

        public void ApplyAllSettings()
        {
            this.ApplyLineEndings();
            this.ApplyParallelImport();
            this.ApplySerializationMode();
        }

        [EditorEvents.InitializeOnLoad]
        private static void InitializeSettings()
        {
            Instance.Initialize();
        }

        private void ApplyLineEndings()
        {
            // Make sure Line Endings are set
            if (this.forceProjectLineEndings && EditorSettings.lineEndingsForNewScripts != Convert(this.projectLineEndings))
            {
                EditorSettings.lineEndingsForNewScripts = Convert(this.projectLineEndings);
            }

            static LineEndingsMode Convert(LineEndings lineEndings)
            {
                switch (lineEndings)
                {
                    case LineEndings.Unix:
                        return LineEndingsMode.Unix;

                    case LineEndings.Windows:
                        return LineEndingsMode.Windows;

                    default:
                        Logger.LogErrorFormat("Found unknown line endings type {0}", lineEndings);
                        return LineEndingsMode.Unix;
                }
            }
        }

        private void ApplyParallelImport()
        {
            if (this.forceParallelImport)
            {
                if (EditorSettings.refreshImportMode != AssetDatabase.RefreshImportMode.OutOfProcessPerQueue)
                {
                    EditorSettings.refreshImportMode = AssetDatabase.RefreshImportMode.OutOfProcessPerQueue;
                }

                if (EditorUserSettings.desiredImportWorkerCount != this.desiredImportWorkerCount)
                {
                    EditorUserSettings.desiredImportWorkerCount = this.desiredImportWorkerCount;
                }

                if (EditorUserSettings.standbyImportWorkerCount != this.standbyImportWorkerCount)
                {
                    EditorUserSettings.standbyImportWorkerCount = this.standbyImportWorkerCount;
                }

                if (EditorUserSettings.idleImportWorkerShutdownDelayMilliseconds / 1000 != this.idleImportWorkerShutdownDelayInSeconds)
                {
                    EditorUserSettings.idleImportWorkerShutdownDelayMilliseconds = this.idleImportWorkerShutdownDelayInSeconds * 1000;
                }
            }
        }

        private void ApplySerializationMode()
        {
            // Make sure Serialization Type is set
            if (this.forceSerializationMode && EditorSettings.serializationMode != this.serializationMode)
            {
                EditorSettings.serializationMode = this.serializationMode;
            }
        }
    }
}
