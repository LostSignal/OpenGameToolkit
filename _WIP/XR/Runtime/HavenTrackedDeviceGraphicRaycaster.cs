#pragma warning disable


using UnityEngine;

#if USING_UNITY_XR_INTERACTION_TOOLKIT
using UnityEngine.XR.Interaction.Toolkit.UI;
#endif

namespace OGT
{
#if USING_UNITY_XR_INTERACTION_TOOLKIT
    public class HavenTrackedDeviceGraphicRaycaster : TrackedDeviceGraphicRaycaster
#else
    public class HavenTrackedDeviceGraphicRaycaster : MonoBehaviour
#endif
    {
        // bool m_IgnoreReversedGraphics;
        // bool m_CheckFor2DOcclusion;
        // bool m_CheckFor3DOcclusion
        // LayerMask m_BlockingMask = -1;
        // QueryTriggerInteraction m_RaycastTriggerInteraction;
    }
}
