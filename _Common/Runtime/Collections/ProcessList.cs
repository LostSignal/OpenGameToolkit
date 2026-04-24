//-----------------------------------------------------------------------
// <copyright file="ProcessList.cs" company="Lost Signal">
//     Copyright (c) Lost Signal. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System;
    using System.Collections.Generic;

    public abstract class ProcessList<T>
    {
        private static readonly OGTLogger Logger = OGTLogger.OGT;

        private readonly Dictionary<long, int> idToIndexMap;
        private readonly string name;

        private T[] items;
        private long[] ids;
        private UnityEngine.Object[] contexts;
        private int count;
        private int currentIndex;

        // Used with RunAllOverXSeconds
        private double elementsToProcess = 0.0;

        public ProcessList(string name, int capacity)
        {
            this.name = name;
            this.items = new T[capacity];
            this.ids = new long[capacity];
            this.contexts = new UnityEngine.Object[capacity];
            this.idToIndexMap = new Dictionary<long, int>(capacity);
        }

        public void Add(long id, T item, UnityEngine.Object context)
        {
            int index = this.count++;

            if (index == this.items.Length)
            {
                Array.Resize(ref this.items, this.items.Length * 2);
                Array.Resize(ref this.ids, this.items.Length * 2);
                Array.Resize(ref this.contexts, this.items.Length * 2);
                Logger.LogWarning($"{this.name} has to grow in capacity!");
            }

            this.items[index] = item;
            this.ids[index] = id;
            this.contexts[index] = context;
            this.idToIndexMap.Add(id, index);
        }

        public void Remove(long id)
        {
            if (this.idToIndexMap.TryGetValue(id, out int indexToRemove) == false)
            {
                return;
            }

            int lastIndex = this.count - 1;

            if (indexToRemove != lastIndex)
            {
                this.items[indexToRemove] = this.items[lastIndex];
                this.contexts[indexToRemove] = this.contexts[lastIndex];

                long lastIndexesId = this.ids[lastIndex];
                this.ids[indexToRemove] = lastIndexesId;
                this.idToIndexMap[lastIndexesId] = indexToRemove;
            }

            this.items[lastIndex] = default;
            this.ids[lastIndex] = default;
            this.contexts[lastIndex] = default;
            this.idToIndexMap.Remove(id);
            this.count--;
        }

        public void RunAll(double maxRuntime = 1000.0)
        {
            this.Run(this.count, maxRuntime);
        }

        public void RunAllOverXSeconds(float deltaTime, float seconds)
        {
            if (deltaTime < 0.001f)
            {
                deltaTime = 0.001f;
            }

            if (seconds < 0.001f)
            {
                seconds = 0.001f;
            }

            double framesPerSecond = 1.0f / deltaTime;
            double elementsPerFrame = (this.count / framesPerSecond) / seconds;
            this.elementsToProcess += elementsPerFrame;

            int elementsToProcessInteger = (int)this.elementsToProcess;

            if (elementsToProcessInteger > 0)
            {
                this.Run(elementsToProcessInteger, 1.0f);
                this.elementsToProcess -= elementsToProcessInteger;
            }
        }

        protected virtual void OnBeforeProcess()
        {
            if (this.currentIndex < 0)
            {
                this.currentIndex = 0;
            }
            else if (this.currentIndex >= this.count)
            {
                this.currentIndex = this.count - 1;
            }
        }

        protected abstract void Process(ref T item);

        private void Run(int maxElements, double maxRuntimeInSeconds)
        {
            if (this.count == 0)
            {
                return;
            }

            this.OnBeforeProcess();

            double endTimeInSeconds = Platform.GetTimeSinceStartup() + maxRuntimeInSeconds;
            maxElements = Math.Min(maxElements, this.count);

            for (int i = 0; i < maxElements; i++)
            {
                try
                {
                    this.Process(ref this.items[this.currentIndex]);
                }
                catch (Exception ex)
                {
                    Logger.LogError($"ProcessList {this.name} caught an exception.", this.contexts[this.currentIndex]);
                    Logger.LogException(ex, this.contexts[this.currentIndex]);
                }

                if (this.count == 0)
                {
                    break;
                }

                // Going through the list backwards in case of items being removed durring processing
                this.currentIndex--;

                if (this.currentIndex < 0)
                {
                    this.currentIndex += this.count;
                }

                if (Platform.GetTimeSinceStartup() >= endTimeInSeconds)
                {
                    break;
                }
            }
        }
    }
}
