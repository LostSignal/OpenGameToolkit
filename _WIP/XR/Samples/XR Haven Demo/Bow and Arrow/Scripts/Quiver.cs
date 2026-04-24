//-----------------------------------------------------------------------
// <copyright file="Quiver.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Lost.Haven
{
    using OGT;
    using OGT.Haven;
    using UnityEngine;
    using UnityEngine.XR.Interaction.Toolkit;
    using UnityEngine.XR.Interaction.Toolkit.Interactables;

    public class Quiver : MonoBehaviour
    {
        [SerializeField] private XRSimpleInteractable grabArea;
        [SerializeField] private Transform arrowHolder;
        [SerializeField] private BowArrow bowArrowPrefab;

#if USING_UNITY_XR_INTERACTION_TOOLKIT
        private void Awake()
        {
            this.grabArea.selectEntered.AddListener(this.OnSelectEntered);
        }

        private void OnDestroy()
        {
            this.grabArea.selectEntered.RemoveListener(this.OnSelectEntered);
        }

        private void OnSelectEntered(SelectEnterEventArgs args)
        {
            var rig = HavenRig.Instance;

            if (rig == null)
            {
                return;
            }

            var arrow = GameObject.Instantiate(this.bowArrowPrefab, this.arrowHolder);
            arrow.transform.Reset();

            var hand = rig.GetHand(args.interactorObject as UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor);

            if (hand != null)
            {
                hand.Deselect(this.grabArea);
                hand.Select(arrow.Grabbable);
            }
        }
#endif
    }
}
