//-----------------------------------------------------------------------
// <copyright file="OGTXRUIInputModule.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.XR
{
    [UnityEngine.AddComponentMenu("")]
#if USING_UNITY_XR_INTERACTION_TOOLKIT
    public class OGTXRUIInputModule : UnityEngine.XR.Interaction.Toolkit.UI.XRUIInputModule
#else
    public class OGTXRUIInputModule : UnityEngine.MonoBehaviour
#endif
    {
    }
}
