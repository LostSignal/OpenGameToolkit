//-----------------------------------------------------------------------
// <copyright file="PickupDoubleTapHandler.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace HavenXR
{
    using OGT;
    using UnityEngine;

    [RequireComponent(typeof(DoubleTapInteractable))]
    public class PickupDoubleTapHandler : MonoBehaviour
    {
        private static readonly OGTLogger Logger = OGTLogger.OGT;

        private DoubleTapInteractable doubleTapInteractable;

        private void Awake()
        {
            this.doubleTapInteractable = this.GetComponent<DoubleTapInteractable>();
            this.doubleTapInteractable.DoubleTappedEvent.AddListener(this.DoubleTapped);
        }

        private void OnDestroy()
        {
            this.doubleTapInteractable.DoubleTappedEvent.RemoveListener(this.DoubleTapped);
        }

        private void DoubleTapped(RaycastHit hit)
        {
            Logger.LogFormat("{0} was double tapped", this.name);
        }
    }
}
