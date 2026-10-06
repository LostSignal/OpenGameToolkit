//-----------------------------------------------------------------------
// <copyright file="AudioChannel.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using OGT.Properties;
    using System.Runtime.CompilerServices;
    using UnityEngine;

    [CreateAssetMenu(menuName = "OGT/Audio/Audio Channel")]
    public class AudioChannel : ScriptableObject
    {
        [SerializeField] private IntProperty volumeProperty;

        public float Volume
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return this.volumeProperty.Value / 100.0f;
            }
        }
    }
}
