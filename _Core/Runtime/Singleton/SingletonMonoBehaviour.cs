//-----------------------------------------------------------------------
// <copyright file="SingletonMonoBehaviour.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System.Runtime.CompilerServices;
    using UnityEngine;

    public abstract class SingletonMonoBehaviour<T> : MonoBehaviour
        where T : MonoBehaviour, ISingleton
    {
        private static T instance;
        private static bool initialized;

        public static T Instance
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => initialized ? instance : CreateInstance();
        }

        public static T Initialize() => Instance;

        private static T CreateInstance()
        {
            instance = SingletonUtil.CreateSingleton<T>(string.Empty);
            instance.gameObject.name = instance.Name;
            initialized = true;

            return instance;
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
                initialized = false;
            }
        }
    }
}
