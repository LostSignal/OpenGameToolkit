//-----------------------------------------------------------------------
// <copyright file="CoroutineRunner.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System.Runtime.CompilerServices;
    using UnityEngine;

    public sealed class CoroutineRunner : MonoBehaviour
    {
        private static CoroutineRunner instance;
        private static bool isInitialized;

        public static CoroutineRunner Instance
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (isInitialized == false)
                {
                    instance = SingletonUtil.CreateSingleton<CoroutineRunner>("Coroutine Runner");
                    isInitialized = true;
                }

                return instance;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetSingleton()
        {
            isInitialized = false;
            instance = null;
        }
    }
}
