//-----------------------------------------------------------------------
// <copyright file="ConcurrentList.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System.Collections.Generic;
    using UnityEngine;

    public class ConcurrentList<T>
    {
        private static readonly OGTLogger Logger = OGTLogger.OGT;

        private readonly object itemsLock = new ();

        [SerializeField]
        private List<T> items = new ();

        public ConcurrentList()
        {
            this.items = new ();
        }

        public ConcurrentList(int capacity)
        {
            this.items = new List<T>(capacity);
        }

        public void Add(T item)
        {
            lock (this.itemsLock)
            {
                if (this.items.Capacity == this.items.Count)
                {
                    Logger.LogWarning("ConcurrentList Had to grow at runtime, consider increasing it's default capacity.");
                }

                this.items.Add(item);
            }
        }

        public void GetItems(List<T> list)
        {
            lock (this.itemsLock)
            {
                list.Clear();
                list.AddRange(this.items);
            }
        }

        public void RemoveItems(List<T> itemsToRemove)
        {
            if (itemsToRemove == null || itemsToRemove.Count == 0)
            {
                return;
            }

            lock (this.itemsLock)
            {
                for (int i = 0; i < itemsToRemove.Count; i++)
                {
                    this.items.Remove(itemsToRemove[i]);
                }
            }
        }
    }
}
