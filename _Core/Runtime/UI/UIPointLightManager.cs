//-----------------------------------------------------------------------
// <copyright file="UIPointLightManager.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    // Lights a group of UI images from a single light source. Every Image gets a shared point light material, and every Shadow is
    // pointed away from the light no matter how its graphic is rotated. This is an editor time tool, the material values and the
    // shadows' effect distances are serialized so nothing needs to happen at runtime.
    [ExecuteInEditMode]
    public class UIPointLightManager : MonoBehaviour
    {
        private static readonly int LightPositionId = Shader.PropertyToID("_LightPosition");
        private static readonly int LightColorId = Shader.PropertyToID("_LightColor");
        private static readonly int LightRadiusId = Shader.PropertyToID("_LightRadius");
        private static readonly int LightFalloffId = Shader.PropertyToID("_LightFalloff");
        private static readonly int AmbientColorId = Shader.PropertyToID("_AmbientColor");

        [Tooltip("Lights the images from, and casts shadows away from, this transform's position")]
        [SerializeField] private Transform lightSource;

        [Header("Lighting")]
        [SerializeField] private Shader lightingShader;
        [Tooltip("Created next to the prefab (or scene) if left empty")]
        [SerializeField] private Material lightingMaterial;
        [ColorUsage(false, true)]
        [SerializeField] private Color lightColor = new Color(1.0f, 0.9f, 0.75f);
        [Tooltip("How far the light reaches, as a fraction of the screen height")]
        [SerializeField] private float lightRadius = 0.75f;
        [SerializeField] private float lightFalloff = 2.0f;
        [SerializeField] private Color ambientColor = new Color(0.35f, 0.33f, 0.38f);
        [SerializeField] private List<Image> images = new List<Image>();

        [Header("Shadows")]
        [SerializeField] private float shadowLength = 10.0f;
        [SerializeField] private List<Shadow> shadows = new List<Shadow>();

#if UNITY_EDITOR

        // "Packages/com.lostsignal.ogt/_Core/Content/UI/Shaders/UI Point Light.shadergraph";
        private const string DefaultShaderGuid = "e1951eac911bbc847aa2e0a9c574fcc9";
        private const float Tolerance = 0.001f;

        private void Reset()
        {
            var defaultShaderPath = UnityEditor.AssetDatabase.GUIDToAssetPath(DefaultShaderGuid);
            this.lightingShader = UnityEditor.AssetDatabase.LoadAssetAtPath<Shader>(defaultShaderPath);
            this.FindImagesAndShadows();
        }

        [ContextMenu("Find Images And Shadows")]
        private void FindImagesAndShadows()
        {
            this.images.Clear();
            this.images.AddRange(this.GetComponentsInChildren<Image>(true));

            this.shadows.Clear();
            this.shadows.AddRange(this.GetComponentsInChildren<Shadow>(true));

            EditorUtil.SetDirty(this);
        }

        private Vector3 lightPosition;

        public void Awake()
        {
            this.lightPosition = this.lightSource != null ? this.lightSource.position : Vector3.zero;
        }

        public void Update()
        {
            var newLightPosition = this.lightSource != null ? this.lightSource.position : Vector3.zero;

            if (Vector3.SqrMagnitude(newLightPosition - this.lightPosition) > 0.1f)
            {
                this.lightPosition = newLightPosition;
                this.UpdateLightingAndShadows();
            }
        }

        public void UpdateLightingAndShadows()
        {
            if (this.lightSource == null)
            {
                return;
            }

            this.UpdateLighting();
            this.UpdateShadows();
        }

        private void UpdateLighting()
        {
            if (this.lightingMaterial == null && this.CreateLightingMaterial() == false)
            {
                return;
            }

            foreach (var image in this.images)
            {
                if (image != null && image.material != this.lightingMaterial)
                {
                    UnityEditor.Undo.RecordObject(image, "Assign Lighting Material");
                    image.material = this.lightingMaterial;
                }
            }

            if (this.TryGetLightScreenPosition(out var lightPosition))
            {
                this.SetIfChanged(LightPositionId, (Vector4)lightPosition);
            }

            this.SetIfChanged(LightColorId, this.lightColor);
            this.SetIfChanged(LightRadiusId, this.lightRadius);
            this.SetIfChanged(LightFalloffId, this.lightFalloff);
            this.SetIfChanged(AmbientColorId, this.ambientColor);
        }

        private bool CreateLightingMaterial()
        {
            // A prefab instance would just end up with an override pointing at a second material, so the prefab itself makes it
            if (this.lightingShader == null || UnityEditor.PrefabUtility.IsPartOfPrefabInstance(this))
            {
                return false;
            }

            var stage = UnityEditor.SceneManagement.PrefabStageUtility.GetPrefabStage(this.gameObject);
            var ownerPath = stage != null ? stage.assetPath : this.gameObject.scene.path;
            var folder = string.IsNullOrEmpty(ownerPath) ? "Assets" : System.IO.Path.GetDirectoryName(ownerPath).Replace('\\', '/');
            var path = UnityEditor.AssetDatabase.GenerateUniqueAssetPath($"{folder}/{this.name} Lighting.mat");

            UnityEditor.Undo.RecordObject(this, "Create Lighting Material");
            this.lightingMaterial = new Material(this.lightingShader);
            UnityEditor.AssetDatabase.CreateAsset(this.lightingMaterial, path);
            Debug.Log($"Created lighting material {path}", this.lightingMaterial);

            return true;
        }

        // The shader works in normalized screen space (0 - 1), which matches the root canvas' rect
        private bool TryGetLightScreenPosition(out Vector2 lightPosition)
        {
            lightPosition = Vector2.zero;

            var canvas = this.GetComponentInParent<Canvas>();

            if (canvas == null)
            {
                return false;
            }

            var canvasRect = (RectTransform)canvas.rootCanvas.transform;

            // A zero scaled canvas (prefabs are saved that way) has no meaningful inverse
            if (Mathf.Abs(canvasRect.lossyScale.x * canvasRect.lossyScale.y) < Mathf.Epsilon)
            {
                return false;
            }

            // Not using Rect.PointToNormalized since it clamps to 0 - 1, and the light is allowed to sit off screen
            Vector2 localPosition = canvasRect.InverseTransformPoint(this.lightSource.position);
            var rect = canvasRect.rect;
            lightPosition = new Vector2((localPosition.x - rect.x) / rect.width, (localPosition.y - rect.y) / rect.height);

            return float.IsNaN(lightPosition.x) == false && float.IsNaN(lightPosition.y) == false;
        }

        // Only touching the material when a value actually changed, otherwise it would be dirtied every frame
        private void SetIfChanged(int id, Vector4 value)
        {
            if ((this.lightingMaterial.GetVector(id) - value).sqrMagnitude > Tolerance * Tolerance)
            {
                UnityEditor.Undo.RecordObject(this.lightingMaterial, "Update Lighting");
                this.lightingMaterial.SetVector(id, value);
            }
        }

        private void SetIfChanged(int id, Color value)
        {
            if (((Vector4)(this.lightingMaterial.GetColor(id) - value)).sqrMagnitude > Tolerance * Tolerance)
            {
                UnityEditor.Undo.RecordObject(this.lightingMaterial, "Update Lighting");
                this.lightingMaterial.SetColor(id, value);
            }
        }

        private void SetIfChanged(int id, float value)
        {
            if (Mathf.Abs(this.lightingMaterial.GetFloat(id) - value) > Tolerance)
            {
                UnityEditor.Undo.RecordObject(this.lightingMaterial, "Update Lighting");
                this.lightingMaterial.SetFloat(id, value);
            }
        }

        private void UpdateShadows()
        {
            // Working with rotations instead of InverseTransformVector so a zero scaled parent (like a hidden panel) can't produce NaNs
            Vector2 toLight = Quaternion.Inverse(this.transform.rotation) * (this.lightSource.position - this.transform.position);

            if (toLight.sqrMagnitude < Tolerance)
            {
                return;
            }

            Vector3 shadowOffset = -toLight.normalized * this.shadowLength;

            foreach (var shadow in this.shadows)
            {
                if (shadow == null)
                {
                    continue;
                }

                // Undoing each graphic's rotation relative to this manager, since Shadow offsets in the graphic's local space
                Vector2 effectDistance = Quaternion.Inverse(shadow.transform.rotation) * this.transform.rotation * shadowOffset;

                // Only touching shadows that actually changed, otherwise the scene would be dirtied every frame
                if ((shadow.effectDistance - effectDistance).sqrMagnitude > Tolerance * Tolerance)
                {
                    UnityEditor.Undo.RecordObject(shadow, "Update Shadow Direction");
                    shadow.effectDistance = effectDistance;
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (this.lightSource != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(this.transform.position, this.lightSource.position);
            }
        }

#endif
    }
}
