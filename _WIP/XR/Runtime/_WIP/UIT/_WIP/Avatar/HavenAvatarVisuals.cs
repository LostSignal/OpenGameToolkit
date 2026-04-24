//-----------------------------------------------------------------------
// <copyright file="HavenAvatarVisuals.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.Haven
{
    using System.Runtime.CompilerServices;
    using OGT.Networking;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    public abstract class HavenAvatarVisuals : MonoBehaviour
    {
#pragma warning disable 0649
        [Header("Network")]
        [SerializeField] private NetworkIdentity networkIdentity;

        [Header("Canvas")]
        [SerializeField] private Canvas avatarCanvas;
        [SerializeField] private TMP_Text displayName;
        [SerializeField] private Vector3 canvasGlobalOffset;

        [Header("Rendering")]
        [SerializeField] private Renderer[] allRenderers;
        [SerializeField] private Renderer[] tintedMeshRenderers;
        [SerializeField] private Graphic[] tintedGraphics;
#pragma warning restore 0649

        protected bool IsOwner
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.networkIdentity.IsOwner;
        }

        public abstract void Serialize(NetworkWriter writer);

        public abstract void Deserialize(NetworkReader reader);

        public virtual void SetAvatarColor(Color avatarColor)
        {
            if (this.tintedMeshRenderers?.Length > 0)
            {
                foreach (var meshRenderer in this.tintedMeshRenderers)
                {
                    meshRenderer.material.color = avatarColor;
                }
            }

            if (this.tintedGraphics?.Length > 0)
            {
                foreach (var graphic in this.tintedGraphics)
                {
                    graphic.color = avatarColor;
                }
            }
        }

        public virtual void SetAvatarName(UserInfo userInfo)
        {
            string avatarName = userInfo.DisplayName;

            if (string.IsNullOrWhiteSpace(avatarName))
            {
                avatarName = $"Player{userInfo.UserHexId.Substring(0, System.Math.Min(4, userInfo.UserHexId.Length))}";
            }

            if (this.displayName)
            {
                this.displayName.text = avatarName;
            }
        }

        protected virtual void ShowVisuals(bool show)
        {
            if (this.allRenderers != null)
            {
                for (int i = 0; i < this.allRenderers.Length; i++)
                {
                    this.allRenderers[i].enabled = show;
                }
            }

            if (this.avatarCanvas)
            {
                this.avatarCanvas.gameObject.SetActive(show);
            }
        }
    }
}
