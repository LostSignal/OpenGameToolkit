//-----------------------------------------------------------------------
// <copyright file="PlayerProximity.cs" company="Lost Signal">
//     Copyright (c) Lost Signal. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if UNITY

namespace OGT
{
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using OGT;
    using UnityEngine;

    public class PlayerProximity : MonoBehaviour, IAwake, IWork
    {
        private static readonly OGTLogger Logger = OGTLogger.OGT;

        private enum State
        {
            Uninitialized,
            InProximity,
            OutOfProximity,
        }

#pragma warning disable 0649
        [ReadOnly][SerializeField] private Transform proximityTransform;
        [SerializeField] private Area area;
        [SerializeField] private bool isDynamic;
        [SerializeField] List<Component> listeners;

        [Header("On Enter Proximity")]
        [SerializeField] List<GameObject> gameObjectsToActivate;
        [SerializeField] List<Behaviour> behavioursToActivate;

        [Header("On Exit Proximity")]
        [SerializeField] List<GameObject> gameObjectsToDeactivate;
        [SerializeField] List<Behaviour> behavioursToDeactivate;
#pragma warning restore 0649

        private PlayerProximityManager playerProximityManager;
        private WorkManager workManager;

        private State currentState;
        private State newState;

        public Transform ProximityTransform
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.proximityTransform;
        }

        public Area Area
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.area;
        }

        public bool IsDynamic
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.isDynamic;
        }

        public void OnAwake(Bootloader bootloader)
        {
            this.workManager = bootloader.FindManager<WorkManager>();
            this.playerProximityManager = bootloader.FindManager<PlayerProximityManager>();
            this.playerProximityManager.Register(this);
        }

        public void UpdateState(bool isInProximity)
        {
            this.newState = isInProximity ? State.InProximity : State.OutOfProximity;

            if (this.currentState != this.newState)
            {
                this.workManager.QueueWork(this);
            }
        }

        public void DoWork()
        {
            if (this.currentState != this.newState)
            {
                this.currentState = this.newState;

                if (this.currentState == State.InProximity)
                {
                    this.OnEnterProximity();
                }
                else
                {
                    this.OnExitProximity();
                }
            }
        }

        public void OnEnterProximity()
        {
            if (this.gameObjectsToActivate?.Count > 0)
            {
                for (int i = 0; i < this.gameObjectsToActivate.Count; i++)
                {
                    this.gameObjectsToActivate[i].SetActive(true);
                }
            }

            if (this.behavioursToActivate?.Count > 0)
            {
                for (int i = 0; i < this.behavioursToActivate.Count; i++)
                {
                    this.behavioursToActivate[i].enabled = true;
                }
            }

            if (this.listeners?.Count > 0)
            {
                for (int i = 0; i < this.listeners.Count; i++)
                {
                    (this.listeners[i] as IPlayerProximityListener)?.OnPlayerEnterProximity();
                }
            }
        }

        public void OnExitProximity()
        {
            if (this.gameObjectsToDeactivate?.Count > 0)
            {
                for (int i = 0; i < this.gameObjectsToDeactivate.Count; i++)
                {
                    this.gameObjectsToDeactivate[i].SetActive(false);
                }
            }

            if (this.behavioursToDeactivate?.Count > 0)
            {
                for (int i = 0; i < this.behavioursToDeactivate.Count; i++)
                {
                    this.behavioursToDeactivate[i].enabled = false;
                }
            }

            if (this.listeners?.Count > 0)
            {
                for (int i = 0; i < this.listeners.Count; i++)
                {
                    (this.listeners[i] as IPlayerProximityListener)?.OnPlayerExitProximity();
                }
            }
        }

        public void AddListener(IPlayerProximityListener listener)
        {
            if (listener is Component component)
            {
                if (this.listeners == null)
                {
                    this.listeners = new List<Component>();
                }

                if (this.listeners.Contains(component) == false)
                {
                    this.listeners.Add(component);
                    EditorUtil.SetDirty(this);
                }
            }
            else
            {
                Logger.LogError("Trying to add an IPlayerProximityListener that is not also a Component");
            }
        }

        public void RemoveListener(IPlayerProximityListener listener)
        {
            if (listener is Component component)
            {
                if (this.listeners != null && this.listeners.Contains(component))
                {
                    this.listeners.Remove(component);
                    EditorUtil.SetDirty(this);
                }
            }
            else
            {
                Logger.LogError("Trying to remove an IPlayerProximityListener that is not also a Component");
            }
        }

        private void Awake() => ActivationManager.Register(this);

        private void OnDestroy()
        {
            if (this.playerProximityManager != null)
            {
                this.playerProximityManager.Unregister(this);
            }
        }

        private void OnValidate()
        {
            if (this.proximityTransform == null)
            {
                this.proximityTransform = this.transform;
                EditorUtil.SetDirty(this.gameObject);
            }

            if (this.area.Size == Vector3.one)
            {
                this.area.Size = new Vector3(10.0f, 3.0f, 10.0f);
                EditorUtil.SetDirty(this.gameObject);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Area.Draw(this.transform, this.area, Color.white.SetA(0.2f));
        }
    }
}

#endif
