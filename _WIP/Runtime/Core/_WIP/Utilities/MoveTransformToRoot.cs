//-----------------------------------------------------------------------
// <copyright file="MoveTransformToRoot.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using UnityEngine;

    public class MoveTransformToRoot : MonoBehaviour
    {
        [EditorEvents.OnProcessSceneBuild]
        private static void OnValidateScene()
        {
            foreach (var moveToRoot in UnityEngine.Object.FindObjectsByType<MoveTransformToRoot>(FindObjectsInactive.Include))
            {
                moveToRoot.MoveToRoot();
            }
        }

#if UNITY_EDITOR
        private void Awake()
        {
            this.MoveToRoot();
        }

#endif

        private void MoveToRoot()
        {
            if (this.transform.parent == null)
            {
                return;
            }

            Transform rootParent = this.transform.parent;

            while (rootParent.parent != null)
            {
                rootParent = rootParent.parent;
            }

            this.transform.SetParent(null);
            this.transform.SetSiblingIndex(rootParent.GetSiblingIndex() + 1);

            GameObject.DestroyImmediate(this);
        }
    }
}
