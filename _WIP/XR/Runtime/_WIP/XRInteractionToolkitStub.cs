#pragma warning disable


#if !USING_UNITY_XR_INTERACTION_TOOLKIT

namespace Unity.XR.CoreUtils
{
    using UnityEngine;

    public class XROrigin : MonoBehaviour
    {
        [SerializeField] private GameObject m_CameraFloorOffsetObject;
        [SerializeField] private float m_CameraYOffset;

        public GameObject CameraFloorOffsetObject => m_CameraFloorOffsetObject;

        public float CameraYOffset => m_CameraYOffset;
    }
}

namespace UnityEngine.XR.Interaction.Toolkit
{
    [System.Serializable]
    public struct InteractionLayerMask
    {
        [SerializeField] private int m_Bits;

        public static implicit operator InteractionLayerMask(int intVal)
        {
            var result = new InteractionLayerMask();
            result.m_Bits = intVal;
            return result;
        }
    }

    public class ActivateEventArgs
    {
    }
}

namespace UnityEngine.XR.Interaction.Toolkit.Interactors
{
    public class XRBaseInteractor : MonoBehaviour
    {
    }

    public class XRSocketInteractor : MonoBehaviour
    {
    }

    public class XRRayInteractor : MonoBehaviour
    {
    }
}

namespace UnityEngine.XR.Interaction.Toolkit.Interactables
{
    using UnityEngine;

    public class XRGrabInteractable : MonoBehaviour
    {
    }

    public class XRSimpleInteractable : MonoBehaviour
    {
    }

    public class XRBaseInteractable : MonoBehaviour
    {
        public enum MovementType
        {
            Instantaneous,
            Kinematic,
            VelocityTracking,
        }
    }

    public enum InteractableSelectMode
    {
        Multiple,
            Single,
    }
}

namespace UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation
{
    public class BaseTeleportationInteractable : MonoBehaviour
    {
    }
}

#endif
