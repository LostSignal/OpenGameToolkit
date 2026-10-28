
namespace OGT
{
    using System.Collections.Generic;
    using Unity.Scripting.LifecycleManagement;
    using UnityEngine;

    public partial class DroppableObject : MonoBehaviour
    {
        [AutoStaticsCleanup]
        private static readonly List<DroppableObject> allDroppableObjects = new(1000);

        [SerializeField]
        private List<Renderer> renderers;

        [SerializeField]
        private int priority;

        public static IReadOnlyList<DroppableObject> AllDroppableObjects => allDroppableObjects;

        public int Priority => this.priority;

        private void OnEnable()
        {
            allDroppableObjects.Add(this);
        }

        private void OnDisable()
        {
            allDroppableObjects.Remove(this);
        }
        
        [InspectorButton]
        public void CollectRenderer()
        {
            foreach (var renderer in this.GetComponentsInChildren<Renderer>())
            {
                if (renderer.enabled)
                {
                    this.renderers.Add(renderer);
                }
            }
        }

        public void Show()
        {
            foreach (var renderer in this.renderers)
            {
                renderer.enabled = true;
            }
        }

        public void Hide()
        {
            foreach (var renderer in this.renderers)
            {
                renderer.enabled = false;
            }
        }

        private void OnValidate()
        {
            if (this.renderers == null)
            {
                this.renderers = new List<Renderer>();
                this.CollectRenderer();
            }
            else if (this.renderers.Count == 0)
            {
                this.CollectRenderer();
            }
        }
    }
}
