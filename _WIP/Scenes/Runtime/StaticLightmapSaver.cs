//-----------------------------------------------------------------------
// <copyright file="StaticLightmapSaver.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

////
//// TODO [bgish]: Do not CopyAndReplaceLightmapTextures if there is no StaticLightmapSaver object in the active scene
////

namespace Lost
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    [ExecuteInEditMode]
    public class StaticLightmapSaver : MonoBehaviour
    {
        private static readonly List<StaticLightmapSaver> recorders = new();

        [SerializeField] private RecordedLightmapData[] recordedLightmaps;
        [SerializeField] private RendererLMInfo[] rendererInfos;

#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
        private static void ApplyLightingRecordersEdtior()
        {
            UnityEditor.SceneManagement.EditorSceneManager.activeSceneChangedInEditMode += (scene1, scene2) =>
            {
                foreach (var recorder in recorders)
                {
                    recorder?.ApplyLighting();
                }
            };

            UnityEditor.Lightmapping.bakeCompleted += () =>
            {
                CopyAndReplaceLightmapTextures();

                foreach (var recorder in recorders)
                {
                    recorder?.RecordLighting();
                    recorder?.ApplyLighting();
                }
            };
        }

        private static void CopyAndReplaceLightmapTextures()
        {
            var newLightmaps = new LightmapData[LightmapSettings.lightmaps.Length];
            int index = 0;

            foreach (var lightmap in LightmapSettings.lightmaps)
            {
                newLightmaps[index] = new LightmapData();


                if (lightmap.lightmapColor != null)
                {
                    var path = UnityEditor.AssetDatabase.GetAssetPath(lightmap.lightmapColor);
                    var newPath = path.Replace(".exr", "_copy.exr");
                    newPath = newPath.Replace(".png", "_copy.png");
                    UnityEditor.AssetDatabase.CopyAsset(path, newPath);
                    newLightmaps[index].lightmapColor = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(newPath);
                    UnityEditor.AssetDatabase.DeleteAsset(path);
                }

                if (lightmap.lightmapDir != null)
                {
                    var path = UnityEditor.AssetDatabase.GetAssetPath(lightmap.lightmapDir);
                    var newPath = path.Replace(".exr", "_copy.exr");
                    newPath = newPath.Replace(".png", "_copy.png");
                    UnityEditor.AssetDatabase.CopyAsset(path, newPath);
                    newLightmaps[index].lightmapDir = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(newPath);
                    UnityEditor.AssetDatabase.DeleteAsset(path);
                }

                if (lightmap.shadowMask != null)
                {
                    var path = UnityEditor.AssetDatabase.GetAssetPath(lightmap.shadowMask);
                    var newPath = path.Replace(".exr", "_copy.exr");
                    newPath = newPath.Replace(".png", "_copy.png");
                    UnityEditor.AssetDatabase.CopyAsset(path, newPath);
                    newLightmaps[index].shadowMask = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(newPath);
                    UnityEditor.AssetDatabase.DeleteAsset(path);
                }

                index++;
            }

            LightmapSettings.lightmaps = newLightmaps;
        }

#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void ApplyLightingRecorders()
        {
            SceneManager.activeSceneChanged += (scene1, scene2) =>
            {
                foreach (var recorder in recorders)
                {
                    recorder?.ApplyLighting();
                }
            };
        }

        private void RecordLighting()
        {
#if UNITY_EDITOR
            if (Application.isPlaying == false && UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() != this.gameObject.scene)
            {
                return;
            }

            this.recordedLightmaps = LightmapSettings.lightmaps.Select(x => new RecordedLightmapData
            {
                lightmapColor = x.lightmapColor,
                lightmapDir = x.lightmapDir,
                lightmapShadowMask = x.shadowMask,
            }).ToArray();

            var allRenderers = GameObject.FindObjectsByType<Renderer>(FindObjectsInactive.Include).OrderBy(x => EntityId.ToULong(x.GetEntityId())).ToArray();

            this.rendererInfos = new RendererLMInfo[allRenderers.Length];

            for (int i = 0; i < allRenderers.Length; i++)
            {
                var renderer = allRenderers[i];
                var lightmapIndex = renderer.lightmapIndex;
                var scaleOffset = renderer.lightmapScaleOffset;
                this.rendererInfos[i] = new RendererLMInfo
                {
                    rendererEntityId = EntityId.ToULong(renderer.GetEntityId()),
                    rendererName = renderer.gameObject.name,
                    lightmapIndex = lightmapIndex,
                    scaleOffset = scaleOffset,
                };
            }

            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        private void ApplyLighting()
        {
#if UNITY_EDITOR
            if (Application.isPlaying == false && UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() != this.gameObject.scene)
            {
                return;
            }
#endif

            if (SceneManager.GetActiveScene() != this.gameObject.scene)
            {
                return;
            }

            if (this.recordedLightmaps == null)
            {
                return;
            }

            LightmapSettings.lightmaps = this.recordedLightmaps.Select(x =>
            {
                var result = new LightmapData();
                result.lightmapColor = x.lightmapColor;
                result.lightmapDir = x.lightmapDir;
                result.shadowMask = x.lightmapShadowMask;
                return result;
            }).ToArray();

            //// TODO [bgish]: Get renderers and make sure to order by instanceid, then walk through both lists at the same time to apply the
            ////               lightmap index and scale offset. This is because the renderer infos may not be in the same order as the renderers
            ////               in the scene, so we need to make sure we are applying the correct lightmap index and scale offset to each renderer.
            foreach (var ri in rendererInfos)
            {
                var rendererGameObject = GameObject.Find(ri.rendererName);
                var renderer = rendererGameObject?.GetComponent<Renderer>();

                if (rendererGameObject && renderer)
                {
                    renderer.lightmapIndex = ri.lightmapIndex;
                    renderer.lightmapScaleOffset = ri.scaleOffset;
                }
            }
        }

        private void Awake() => this.ApplyLighting();

        private void OnEnable() => recorders.Add(this);

        private void OnDisable() => recorders.Remove(this);

        [System.Serializable]
        private class RendererLMInfo
        {
            public string rendererName;
            public ulong rendererEntityId;
            public int lightmapIndex;
            public Vector4 scaleOffset;
        }

        [System.Serializable]
        private class RecordedLightmapData
        {
            public Texture2D lightmapColor;
            public Texture2D lightmapDir;
            public Texture2D lightmapShadowMask;
        }

#if UNITY_EDITOR
        [UnityEditor.CustomEditor(typeof(StaticLightmapSaver))]
        private class LightingRecorderEditor : UnityEditor.Editor
        {
            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();

                if (GUILayout.Button("Set Active Scane and Bake Lighting"))
                {
                    // Making sure this is the active scene before baking
                    var lightingRecorder = this.target as StaticLightmapSaver;
                    UnityEditor.SceneManagement.EditorSceneManager.SetActiveScene(lightingRecorder.gameObject.scene);

                    // Start the lighting bake
                    UnityEditor.Lightmapping.BakeAsync();
                }
            }
        }
#endif
    }
}
