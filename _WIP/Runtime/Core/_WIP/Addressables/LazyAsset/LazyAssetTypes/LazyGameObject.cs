//-----------------------------------------------------------------------
// <copyright file="LazyGameObject.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if !UNITY

namespace OGT
{
    using System;
    using UnityEngine;

    [Serializable]
    #if UNITY
    public class LazyGameObject : LazyAsset<GameObject>
    #else
    public class LazyGameObject : LazyAsset<object>
    #endif
    {
    }
}

#endif
