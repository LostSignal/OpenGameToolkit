namespace OGT
{
    using UnityEditor;
    using UnityEngine;

    public static class ObjectExtensions
    {
        private enum InspectorObjectType
        {
            ScriptableObject,
            Prefab,
            PrefabStage,
            SceneObject,
        }

        public static string EditorGetGuid(this Object obj)
        {
#if UNITY_EDITOR
            var assetPath = obj.EditorGetAssetPath();

            if (string.IsNullOrEmpty(assetPath))
            {
                return null;
            }

            return UnityEditor.AssetDatabase.GUIDFromAssetPath(assetPath).ToString();
#else
            throw new System.Exception("ObjectExtension.EditorGetGuid called in payer build!");
#endif
        }

        public static string EditorGetAssetPath(this Object obj)
        {
#if UNITY_EDITOR
            return UnityEditor.AssetDatabase.GetAssetPath(obj);
#else
            throw new System.Exception("ObjectExtension.EditorGetAssetPath called in payer build!");
#endif
        }

        public static bool EditorIsSceneObject(this Object obj)
        {
#if UNITY_EDITOR
            if (obj == null)
            {
                return false;
            }

            bool isSceneType = obj is GameObject || obj is Component;

            if (isSceneType)
            {
                return EditorIsPrefab(obj) ? false : true;
            }
            else
            {
                return false;
            }
#else
            throw new System.Exception("ObjectExtension.IsSceneObject called in payer build!");
#endif
        }

        public static bool EditorIsPrefab(this Object obj)
        {
#if UNITY_EDITOR
            var inspectorType = GetInspectorObjectType(obj);

            return obj == null ? false : (inspectorType == InspectorObjectType.Prefab || inspectorType == InspectorObjectType.PrefabStage);
#else
            throw new System.Exception("ObjectExtension.IsPrefab called in payer build!");
#endif
        }

        public static bool EditorIsAddressable(this Object obj)
        {
#if UNITY_EDITOR
            var settings = UnityEditor.AddressableAssets.AddressableAssetSettingsDefaultObject.Settings;
            var entry = settings.FindAssetEntry(UnityEditor.AssetDatabase.AssetPathToGUID(UnityEditor.AssetDatabase.GetAssetPath(obj)));
            return entry != null;
#else
            throw new System.Exception("ObjectExtension.EditorIsAddressable called in payer build!");
#endif
        }

        private static InspectorObjectType GetInspectorObjectType(UnityEngine.Object target)
        {
#if UNITY_EDITOR
            if (target is ScriptableObject)
            {
                return InspectorObjectType.ScriptableObject;
            }
            else if (PrefabUtility.IsPartOfPrefabAsset(target))
            {
                return InspectorObjectType.Prefab;
            }
            else if (target is Component component)
            {
                var rootGameObject = GetRootGameObject(component.gameObject);
                var scene = component.gameObject.scene;

                if (string.IsNullOrEmpty(scene.path) && scene.name == rootGameObject.name)
                {
                    return InspectorObjectType.PrefabStage;
                }
                else if (scene.IsValid())
                {
                    return InspectorObjectType.SceneObject;
                }
            }

            throw new System.Exception("Unknown Inspector Object Type!");

            static GameObject GetRootGameObject(GameObject gameObject)
            {
                return gameObject.transform.parent != null ?
                    GetRootGameObject(gameObject.transform.parent.gameObject) :
                    gameObject;
            }

#else
            throw new System.Exception("ObjectExtension.GetInspectorObjectType called in payer build!");
#endif
        }
    }
}
