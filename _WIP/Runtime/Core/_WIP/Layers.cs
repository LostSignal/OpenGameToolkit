//-----------------------------------------------------------------------
// <copyright file="Layers.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using UnityEngine;

    public static class Layers
    {
        private static readonly OGTLogger Logger = OGTLogger.OGT;

        private static int defaultLayer;
        private static int actorLayer;
        private static int interactableLayer;
        private static int interactorLayer;
        private static int teleportLayer;

        public static int Default
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => defaultLayer;
        }

        public static int Actor
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => actorLayer;
        }

        public static int Interactable
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => interactableLayer;
        }

        public static int Interactor
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => interactorLayer;
        }

        public static int Teleport
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => teleportLayer;
        }

        public static void SetInteractable(List<Collider> colliders)
        {
            SetLayer(colliders, Interactable);
        }

        public static void SetTeleport(List<Collider> colliders)
        {
            SetLayer(colliders, Teleport);
        }

        private static void SetLayer(List<Collider> colliders, int layerIndex)
        {
            if (Application.isPlaying)
            {
                Logger.LogError("Layers.SetInteractable being called at runtime!");
                return;
            }

            if (colliders == null || colliders.Count == 0 || layerIndex == -1)
            {
                return;
            }

#if UNITY_EDITOR
            // Making sure all colliders are on the right layer
            foreach (var collider in colliders)
            {
                if (collider.gameObject.layer != layerIndex)
                {
                    UnityEditor.EditorApplication.delayCall += () =>
                    {
                        if (collider && collider.gameObject)
                        {
                            collider.gameObject.layer = layerIndex;
                            EditorUtil.SetDirty(collider.gameObject);
                        }
                    };
                }
            }
#endif
        }

        static Layers()
        {
            InitializeLayers();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void InitializeLayers()
        {
            defaultLayer = LayerMask.NameToLayer("Default");
            teleportLayer = InitializeLayer("Teleport");
            interactorLayer = InitializeLayer("Interactor");
            interactableLayer = InitializeLayer("Interactable");
            actorLayer = InitializeLayer("Actor");

            SetIgnoreLayerCollision(Default, Actor, false);
            SetIgnoreLayerCollision(Default, Interactable, false);
            SetIgnoreLayerCollision(Default, Interactor, false);
            SetIgnoreLayerCollision(Default, Teleport, true);

            SetIgnoreLayerCollision(Interactable, Interactor, false);
            SetIgnoreLayerCollision(Interactable, Interactable, false);
            SetIgnoreLayerCollision(Interactor, Interactor, false);
        }

        private static int InitializeLayer(string layerName)
        {
            int layer = LayerMask.NameToLayer(layerName);

            if (layer != -1)
            {
                return layer;
            }

            layer = AddLayerToSettings(layerName);

            if (layer == -1)
            {
                Logger.LogError($"Unable to find layer {layerName}! App will not work correctly. Please add this layer to your settings.");
            }

            return layer;

            static int AddLayerToSettings(string layerName)
            {
#if UNITY_EDITOR
                var tagManagerAsset = UnityEditor.AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0];
                var tagManager = new UnityEditor.SerializedObject(tagManagerAsset);
                var layers = tagManager.FindProperty("layers");
                int layerSize = layers.arraySize;

                for (int i = layerSize - 1; i >= 8; i--)
                {
                    var element = layers.GetArrayElementAtIndex(i);

                    if (string.IsNullOrEmpty(element.stringValue))
                    {
                        element.stringValue = layerName;
                        tagManager.ApplyModifiedPropertiesWithoutUndo();
                        EditorUtil.SetDirty(tagManagerAsset);

                        return i;
                    }
                }
#endif

                return -1;
            }
        }

        private static void SetIgnoreLayerCollision(int layer1, int layer2, bool ignore)
        {
            if (Physics.GetIgnoreLayerCollision(layer1, layer2) != ignore)
            {
                Physics.IgnoreLayerCollision(layer1, layer2, ignore);
            }
        }
    }
}
