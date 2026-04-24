//-----------------------------------------------------------------------
// <copyright file="ConsoleUtil.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System;
    using System.Reflection;
    using UnityEditor;

    public static class ConsoleUtil
    {
        private static MethodInfo clearConsoleMethod;

        public static void Clear()
        {
            if (clearConsoleMethod == null)
            {
                Assembly assembly = Assembly.GetAssembly(typeof(SceneView));
                Type logEntries = assembly.GetType("UnityEditor.LogEntries");
                clearConsoleMethod = logEntries.GetMethod("Clear");
            }

            clearConsoleMethod.Invoke(new object(), null);
        }
    }
}
