
namespace OGT
{ 
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Events;

    public class DropObjects : MonoBehaviour
    {
        [SerializeField] private Transform worldTransform;
        [SerializeField] private float delayBeforeDrop = 0.0f;
        [SerializeField] private float dropHeight = 5.0f;
        [SerializeField] private float dropDelay = 0.1f;
        [SerializeField] private float dropTime = 1.0f;
        [SerializeField] private int dropsPerFrame = 1;
        [SerializeField] private AnimationCurve dropCurve;
        [SerializeField] private UnityEvent onDropComplete;

        public void DemoReset() => this.HideDroppableObjects();

        private void HideDroppableObjects()
        {
            foreach (var droppableObject in DroppableObject.AllDroppableObjects)
            {
                droppableObject.Hide();
            }
        }

        // Sorts by priority (lowest first), then by distance from the top-left corner (min x, max z) of the board
        private List<DroppableObject> GetSortedDroppableObjects()
        {
            var droppableObjects = new List<DroppableObject>(DroppableObject.AllDroppableObjects);
            var localPositions = new Dictionary<DroppableObject, Vector3>(droppableObjects.Count);

            float minX = float.MaxValue;
            float maxZ = float.MinValue;

            foreach (var droppableObject in droppableObjects)
            {
                Vector3 position = droppableObject.transform.position;
                Vector3 localPosition = this.worldTransform != null ? this.worldTransform.InverseTransformPoint(position) : position;
                localPositions.Add(droppableObject, localPosition);

                minX = Mathf.Min(minX, localPosition.x);
                maxZ = Mathf.Max(maxZ, localPosition.z);
            }

            droppableObjects.Sort((a, b) =>
            {
                int priorityCompare = a.Priority.CompareTo(b.Priority);
                return priorityCompare != 0 ? priorityCompare : DistanceToTopLeft(a).CompareTo(DistanceToTopLeft(b));
            });

            return droppableObjects;

            float DistanceToTopLeft(DroppableObject droppableObject)
            {
                Vector3 localPosition = localPositions[droppableObject];
                float x = localPosition.x - minX;
                float z = localPosition.z - maxZ;
                return (x * x) + (z * z);
            }
        }

        [InspectorButton]
        public Coroutine Play()
        {
            return this.StartCoroutine(DropCoroutine());

            IEnumerator DropCoroutine()
            {
                this.HideDroppableObjects();

                yield return null;

                List<DroppableObject> sortedDroppableObjects = this.GetSortedDroppableObjects();

                yield return new WaitForSeconds(this.delayBeforeDrop);

                // TODO [bgish]: Actually put an animation on all these objects and have them drop from the sky
                int drops = 0;
                foreach (var droppableObject in sortedDroppableObjects)
                {
                    this.StartCoroutine(DropObject(droppableObject));
                    drops++;

                    if (drops >= this.dropsPerFrame)
                    {
                        yield return new WaitForSeconds(this.dropDelay);
                        drops = 0;
                    }
                }

                yield return new WaitForSeconds(this.dropTime);

                this.onDropComplete?.Invoke();
            }

            IEnumerator DropObject(DroppableObject droppableObject)
            {
                droppableObject.Show();

                Transform transform = droppableObject.transform;

                Vector3 endWorldPosition = transform.position;
                Vector3 startWorldPosition = transform.position + (Vector3.up * this.dropHeight);

                Vector3 endRelativePosition = this.worldTransform.InverseTransformPoint(endWorldPosition);
                Vector3 startRelativePosition = this.worldTransform.InverseTransformPoint(startWorldPosition);

                float elapsedTime = 0.0f;

                while (true)
                {
                    var progress = Mathf.Clamp01(elapsedTime / this.dropTime);
                    var newRelativePosition = Vector3.Lerp(startRelativePosition, endRelativePosition, this.dropCurve.Evaluate(progress));
                    transform.position = this.worldTransform.TransformPoint(newRelativePosition);

                    if (progress >= 1f)
                    {
                        break;
                    }

                    yield return null;

                    elapsedTime += Time.deltaTime;
                }
            }
        }
    }
}
