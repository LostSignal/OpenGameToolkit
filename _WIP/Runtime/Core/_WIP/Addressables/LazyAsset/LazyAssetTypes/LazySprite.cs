//-----------------------------------------------------------------------
// <copyright file="LazySprite.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System;
    using UnityEngine;

    [Serializable]
    #if UNITY
    public class LazySprite : LazyAssetT<Sprite>
    #else
    public class LazySprite : LazyAsset<object>
    #endif
    {
        #if UNITY

        public LazySprite()
        {
        }

        public LazySprite(string guid)
            : base(guid)
        {
        }

        #endif
    }
}
