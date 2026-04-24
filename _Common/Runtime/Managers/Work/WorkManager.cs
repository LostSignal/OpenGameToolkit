//-----------------------------------------------------------------------
// <copyright file="WorkManager.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if UNITY

namespace OGT
{
    using System.Threading.Tasks;
    using UnityEngine;

    public sealed class WorkManager : Manager, IAwake, IUpdate
    {
#pragma warning disable 0649
        [SerializeField] private double maxRuntimeInMilliseconds;
#pragma warning restore 0649

        private static readonly OGTLogger Logger = OGTLogger.OGT;

        private Queue<IWork> workQueue = new Queue<IWork>(1000);

        public int UpdateOrder => 3;

        protected override Task InitializeManager(Bootloader bootloader)
        {
            return Task.CompletedTask;
        }

        public override void ResetToDefaults()
        {
            this.maxRuntimeInMilliseconds = 0.5f;
        }

        public void OnAwake(Bootloader bootloader)
        {
            this.workQueue.OnGrow += () => Logger.LogError("Work Manager Queue Grew!");
        }

        public void OnUpdate(float deltaTime)
        {
            double endTime = Platform.GetTimeSinceStartup() + (this.maxRuntimeInMilliseconds / 1000.0);

            while (this.workQueue.Count > 0)
            {
                // TODO [bgish]: Do a bunch of safty checks and limit the time
                var work = this.workQueue.Dequeue();
                work.DoWork();


                if (Platform.GetTimeSinceStartup() > endTime)
                {
                    break;
                }
            }
        }

        public void QueueWork(IWork work)
        {
            this.workQueue.Enqueue(work);
        }

        public void QueueImportantWork(IWork work)
        {
            this.workQueue.EnqueueFront(work);
        }
    }
}

#endif
