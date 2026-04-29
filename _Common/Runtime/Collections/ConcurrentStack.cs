//-----------------------------------------------------------------------
// <copyright file="ConcurrentStack.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System.Collections.Generic;
    using UnityEngine;

    public class ConcurrentStack<T>
    {
        private readonly object itemsLock = new();

        [SerializeField]
        private Stack<T> items = new();

        public void Push(T t)
        {
            lock (this.itemsLock)
            {
                this.items.Push(t);
            }
        }

        public bool TryPop(out T t)
        {
            lock (this.itemsLock)
            {
                if (this.items.Count > 0)
                {
                    t = this.items.Pop();
                    return true;
                }
                else
                {
                    t = default;
                    return false;
                }
            }
        }
    }
}
