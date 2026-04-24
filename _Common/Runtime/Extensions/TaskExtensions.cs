//-----------------------------------------------------------------------
// <copyright file="TaskExtensions.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System.Collections;
    using System.Threading.Tasks;

    public static class TaskExtensions
    {
        public static IEnumerator AsIEnumerator(this Task task)
        {
            while (task.IsCompleted == false)
            {
                yield return null;
            }
        }
    }
}
