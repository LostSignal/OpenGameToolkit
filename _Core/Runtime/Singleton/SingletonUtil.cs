//-----------------------------------------------------------------------
// <copyright file="SingletonUtil.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using UnityEngine;

    public static class SingletonUtil
    {
        private static GameObject rootInstance;

        public static GameObject SingletonRoot
        {
            get
            {
                if (rootInstance == null)
                {
                    rootInstance = new GameObject("OGT - Singletons");
                    GameObject.DontDestroyOnLoad(rootInstance);
                    rootInstance.transform.Reset();
                }

                return rootInstance;
            }
        }

        public static T CreateSingleton<T>(string name)
            where T : MonoBehaviour
        {
            if (Platform.IsApplicationQuitting)
            {
                return null;
            }

            var singleton = new GameObject(name, typeof(T));
            singleton.transform.SetParent(SingletonRoot.transform);
            singleton.transform.Reset();

            return singleton.GetComponent<T>();
        }
    }
}
