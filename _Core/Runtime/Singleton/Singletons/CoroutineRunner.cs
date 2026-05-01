//-----------------------------------------------------------------------
// <copyright file="CoroutineRunner.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using UnityEngine;

namespace OGT
{
    public sealed class CoroutineRunner : SingletonMonoBehaviour<CoroutineRunner>, ISingleton
    {
        string ISingleton.Name => "Coroutine Runner";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetSingleton() => ResetStatics();
    }
}
