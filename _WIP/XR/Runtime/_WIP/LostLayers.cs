
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public static class LostLayers
{
    public static InteractionLayerMask Teleport => ~0;

    public static void SetInteractable(List<Collider> colliders)
    {
    }

    public static void SetTeleport(List<Collider> colliders)
    {
    }
}
