//-----------------------------------------------------------------------
// <copyright file="ConcurrentQueue.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using UnityEngine;

    public class ConcurrentQueue<T>
    {
        private readonly object itemsLock = new();

        [SerializeField]
        private System.Collections.Generic.Queue<T> items = new();

        public bool TryDequeue(out T output)
        {
            lock (this.itemsLock)
            {
                if (this.items.Count > 0)
                {
                    output = this.items.Dequeue();
                    return true;
                }
                else
                {
                    output = default;
                    return false;
                }
            }
        }

        public void Enqueue(T t)
        {
            lock (this.itemsLock)
            {
                this.items.Enqueue(t);
            }
        }
    }
}
