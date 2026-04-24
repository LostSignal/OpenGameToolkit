//-----------------------------------------------------------------------
// <copyright file="Queue.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System;
    using System.Runtime.CompilerServices;

    public class Queue<T>
    {
        private static readonly T DefaultElement = default;

        private T[] elements;
        private int capacity;
        private int startIndex;
        private int count;
        private Action grow;

        public Queue(int initialCapacity)
        {
            this.capacity = initialCapacity;
            this.elements = new T[initialCapacity];
            this.startIndex = 0;
            this.count = 0;
        }

        public event Action OnGrow
        {
            add => this.grow += value;
            remove => this.grow -= value;
        }

        public int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.count;
        }

        public bool IsEmpty
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.count == 0;
        }

        public int EnqueueFront(T element)
        {
            if (this.Count == this.capacity)
            {
                this.Grow();
                this.grow?.Invoke();
            }

            this.startIndex = this.startIndex == 0 ? this.capacity - 1 : this.startIndex - 1;
            this.elements[this.startIndex] = element;
            this.count++;

            return this.startIndex;
        }

        public int Enqueue(T element)
        {
            if (this.Count == this.capacity)
            {
                this.Grow();
                this.grow?.Invoke();
            }

            int index = (this.startIndex + this.count) % this.capacity;
            this.elements[index] = element;
            this.count++;

            return index;
        }

        public T Dequeue()
        {
            if (this.count == 0)
            {
                throw new IndexOutOfRangeException();
            }

            T value = this.elements[this.startIndex];

            this.elements[this.startIndex] = DefaultElement;
            this.startIndex = (this.startIndex + 1) % this.capacity;
            this.count--;

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetElementAt(int index)
        {
            return this.elements[index];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DeleteElementAt(int index)
        {
            this.elements[index] = DefaultElement;
        }

        public void Clear()
        {
            Array.Clear(this.elements, 0, this.elements.Length);
            this.startIndex = 0;
            this.count = 0;
        }

        private void Grow()
        {
            int newCapacity = this.capacity * 2;
            var newQueue = new Queue<T>(newCapacity);

            while (this.Count > 0)
            {
                newQueue.Enqueue(this.Dequeue());
            }

            this.capacity = newQueue.capacity;
            this.elements = newQueue.elements;
            this.startIndex = newQueue.startIndex;
            this.count = newQueue.count;
        }
    }
}
