
using System.Collections.Generic;

namespace OGT
{
    public abstract class GameBehavior
#if UNITY_6000_0_OR_NEWER
        : UnityEngine.MonoBehaviour
#endif
    {
#if UNITY_6000_0_OR_NEWER
        private void Awake() => ActivationManager.Register(this);
#endif

        public bool IsEnabled
        {
#if UNITY_6000_0_OR_NEWER
            get => this.enabled;
#else
            throw new System.NotImplementedException();
#endif
        }

        public IEnumerable<T> GetChildrenOfType<T>()
        {
#if UNITY_6000_0_OR_NEWER
            var children = this.gameObject.GetComponentsInChildren<T>();

            foreach (var child in children)
            {
                yield return child;
            }
#else
            throw new System.NotImplementedException();
#endif
        }
    }
}
