//-----------------------------------------------------------------------
// <copyright file="TypeUtil.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    public static class TypeUtil
    {
        private static readonly Dictionary<Type, HashSet<Type>> TypesCache = new();

        [Obsolete("GetAllTypesOf is deprecated, please use Unity's built in TypeCache instead.", true)]
        public static IEnumerable<Type> GetAllTypesOf<T>(bool ignoreUnityAssemblies = true)
        {
            Type type = typeof(T);

            if (TypesCache.TryGetValue(type, out HashSet<Type> types) == false)
            {
                lock (string.Intern(type.FullName))
                {
                    if (TypesCache.TryGetValue(type, out types) == false)
                    {
                        types = new HashSet<Type>();

                        foreach (System.Reflection.Assembly assembly in System.AppDomain.CurrentDomain.GetAssemblies())
                        {
                            if (ignoreUnityAssemblies && IsUnityAssembly(assembly))
                            {
                                continue;
                            }

                            foreach (Type assemblyType in assembly.GetTypes())
                            {
                                if (type.IsAssignableFrom(assemblyType) &&
                                    assemblyType.IsInterface == false &&
                                    assemblyType.IsAbstract == false &&
                                    types.Contains(assemblyType) == false)
                                {
                                    types.Add(assemblyType);
                                }
                            }
                        }

                        TypesCache.Add(type, types);
                    }
                }
            }

            foreach (Type t in types)
            {
                yield return t;
            }
        }

        [Obsolete("GetTypeByName is deprecated, please use Unity's built in TypeCache instead.", true)]
        public static Type GetTypeByName<TBaseType>(string typeName)
        {
            foreach (Type t in TypeUtil.GetAllTypesOf<TBaseType>())
            {
                if (t.Name == typeName)
                {
                    return t;
                }
            }

            return null;
        }

        [Obsolete("IsUnityAssembly is deprecated, please use Unity's built in TypeCache instead.", true)]
        public static bool IsUnityAssembly(Assembly assembly)
        {
            string name = assembly.FullName.Substring(0, assembly.FullName.IndexOf(","));

            return name == "System" ||
                   name == "mscorlib" ||
                   name == "UnityEngine" ||
                   name == "UnityEditor" ||
                   name == "Shouldly" ||
                   name == "nunit.framework" ||
                   name == "Newtonsoft.Json" ||
                   name == "PlayerBuildProgramLibrary.Data" ||
                   name == "Purchasing.Common" ||
                   name == "PPv2URPConverters" ||
                   name == "netstandard" ||
                   name.StartsWith("Bee.") ||
                   name.StartsWith("Mono.") ||
                   name.StartsWith("Unity.") ||
                   name.StartsWith("System.") ||
                   name.StartsWith("UnityEditor.") ||
                   name.StartsWith("UnityEngine.");
        }
    }
}
