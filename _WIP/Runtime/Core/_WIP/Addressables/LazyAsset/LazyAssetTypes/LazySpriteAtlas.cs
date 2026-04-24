//-----------------------------------------------------------------------
// <copyright file="LazySpriteAtlas.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System;

    #if UNITY
    using UnityEngine.U2D;
    #endif

    [Serializable]
    #if UNITY
    public class LazySpriteAtlas : LazyAssetT<SpriteAtlas>
    #else
    public class LazySpriteAtlas : LazyAsset<object>
    #endif
    {
        #if UNITY
        public LazySpriteAtlas()
        {
        }

        public LazySpriteAtlas(string guid)
            : base(guid)
        {
        }

        #endif
    }
}
