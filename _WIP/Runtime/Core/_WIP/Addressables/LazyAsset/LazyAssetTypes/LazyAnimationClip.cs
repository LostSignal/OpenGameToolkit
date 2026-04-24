//-----------------------------------------------------------------------
// <copyright file="LazyGameObject.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System;

    [Serializable]
#if UNITY
    public class LazyAnimationClip : LazyAssetT<UnityEngine.AnimationClip>
#else
    public class LazyAnimationClip : LazyAsset<object>
#endif
    {
    }
}
