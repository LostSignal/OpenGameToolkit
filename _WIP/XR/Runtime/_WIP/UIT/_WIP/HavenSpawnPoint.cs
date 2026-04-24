#pragma warning disable

//-----------------------------------------------------------------------
// <copyright file="HavenSpawnPoint.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.Haven
{
    using UnityEngine;

    [AddComponentMenu("Haven XR/HXR Spawn Point")]
    public class HavenSpawnPoint : GameBehavior, IStart
    {
        public void OnStart()
        {
#if USING_UNITY_XR_INTERACTION_TOOLKIT
            HavenRig.Instance.transform.position = this.transform.position;
#endif
        }
    }
}
