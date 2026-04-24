//-----------------------------------------------------------------------
// <copyright file="UnityTask.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Heavily inspired by TwistedOakStudios SpecialCoroutines.
    /// https://github.com/TwistedOakStudios/TOUnityUtilities/tree/master/Assets/TOUtilities.
    /// https://www.youtube.com/watch?v=ciDD6Wl-Evk.
    /// </summary>
    /// <typeparam name="T">The return type of the task.</typeparam>
    public class UnityTask<T> : CustomYieldInstruction, IUnityTask
    {
        private readonly double timeoutInSeconds;
        private readonly double startTime;
        private T value;

        private UnityTask(MonoBehaviour gameObject, IEnumerator<T> coroutine, double timeoutInSeconds)
        {
            this.startTime = Time.realtimeSinceStartupAsDouble;
            this.timeoutInSeconds = timeoutInSeconds;
            gameObject.StartCoroutine(this.InternalCoroutine(coroutine));
        }

        private UnityTask(T value)
        {
            this.Exception = null;
            this.IsCanceled = false;
            this.DidTimeout = false;
            this.IsDone = true;
            this.value = value;
        }

        public Exception Exception { get; private set; }

        public bool HasError
        {
            get { return this.Exception != null; }
        }

        public bool IsCanceled { get; private set; }

        public bool DidTimeout { get; private set; }

        public bool IsDone { get; private set; }

        public T Value
        {
            get
            {
                if (this.HasError)
                {
                    throw this.Exception;
                }

                return this.value;
            }
        }

        public override bool keepWaiting
        {
            get { return this.IsDone == false; }
        }

        public static UnityTask<T> Empty(T value)
        {
            return new UnityTask<T>(value);
        }

        public static UnityTask<T> Run(IEnumerator<T> coroutine, float timeoutInSeconds = float.MaxValue)
        {
            return new UnityTask<T>(CoroutineRunner.Instance, coroutine, timeoutInSeconds);
        }

        public void Cancel()
        {
            this.IsCanceled = true;
            this.Exception = new UnityTaskCanceledException();
        }

        private IEnumerator<T> InternalCoroutine(IEnumerator<T> coroutine)
        {
            while (true)
            {
                // checking if the user canceled it
                if (this.IsCanceled)
                {
                    yield break;
                }

                try
                {
                    if (coroutine.MoveNext() == false)
                    {
                        this.IsDone = true;
                        yield break;
                    }
                }
                catch (Exception ex)
                {
                    this.Exception = ex;
                    this.IsDone = true;
                    yield break;
                }

                // Checking for timeout
                if (Time.realtimeSinceStartupAsDouble - this.startTime > this.timeoutInSeconds)
                {
                    this.Exception = new UnityTaskTimeoutException();
                    this.DidTimeout = true;
                    this.IsDone = true;
                    yield break;
                }

                this.value = coroutine.Current;

                yield return this.value;
            }
        }
    }
}
