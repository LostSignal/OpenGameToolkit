//-----------------------------------------------------------------------
// <copyright file="ActivationManager.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using UnityEngine;

#if UNITY_6000_0_OR_NEWER
    [DefaultExecutionOrder(-1000)]
#endif
    public sealed class ActivationManager : Manager, ILevelLoadPreprocessor
    {
        private static readonly OGTLogger Logger = OGTLogger.OGT;
        private static Queue<object> objects;
        private static bool isProcessing;

#pragma warning disable 0649
        [SerializeField] private double maxActivationTimeInMillis;
        [SerializeField] private int awakesInitialCapacity;
        [SerializeField] private int startsInitialCapacity;
        [SerializeField] private int updatesInitialCapacity;
        [SerializeField] private int lateUpdatesInitialCapacity;
        [SerializeField] private int fixedUpdatesInitialCapacity;
        [SerializeField] private int runningLateUpdatesInitialCapacity;
        [SerializeField] private int runningFixedUpdatesInitialCapacity;
        [SerializeField] private int runningUpdatesInitialCapacity;
#pragma warning restore 0649

        private Queue<IAwake> awakes;
        private Queue<IStart> starts;
        private Queue<IUpdate> updates;
        private Queue<ILateUpdate> lateUpdates;
        private Queue<IFixedUpdate> fixedUpdates;
        private List<ILateUpdate> runningLateUpdates;
        private List<IFixedUpdate> runningFixedUpdates;
        private List<IUpdate> runningUpdates;
        private LevelManager levelManager;
        private Bootloader bootloader;
        private bool updatesNeedReorder;
        private bool isPaused;

        public bool IsPaused
        {
            get => this.isPaused;
            set => this.isPaused = value;
        }

        public bool IsProcessing => isProcessing;

#if UNITY_6000_0_OR_NEWER
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void InitializeOnLoad()
        {
            objects = new Queue<object>(1000);
            isProcessing = false;
        }
#else
        static ActivationManager()
        {
            objects = new Queue<object>(1000);
            isProcessing = false;
        }        
#endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Register(object obj)
        {
            isProcessing = true;
            objects.Enqueue(obj);
        }

        public override void ResetToDefaults()
        {
            this.maxActivationTimeInMillis = 0.5;
            this.awakesInitialCapacity = 1000;
            this.startsInitialCapacity = 1000;
            this.updatesInitialCapacity = 100;
            this.lateUpdatesInitialCapacity = 100;
            this.fixedUpdatesInitialCapacity = 100;
            this.runningLateUpdatesInitialCapacity = 100;
            this.runningFixedUpdatesInitialCapacity = 100;
            this.runningUpdatesInitialCapacity = 100;
        }

        public override void OnManagerDestroyed()
        {
            if (this.levelManager != null)
            {
                this.levelManager.RemoveLevelLoadPreprocessor(this);
            }

            objects = null;
            isProcessing = false;
        }

        private void OnPlatformUpdate(object sender, System.EventArgs e)
        {
            if (this.isPaused)
            {
                return;
            }

            if (isProcessing)
            {
                this.ProcessActivationRequests();
            }

            if (this.updatesNeedReorder)
            {
                //// TODO [bgish]: Do Better, this is temp
                this.runningUpdates = this.runningUpdates.OrderBy(x => x.UpdateOrder).ToList();
                this.updatesNeedReorder = false;
            }

            float deltaTime = Platform.GetDeltaTime();

            for (int i = 0; i < this.runningUpdates.Count; i++)
            {
                //// if this is a bad reference, then remove it (and swap with last one in the list)
                //// if not enabled, then skip it
                //// keep stats on worst offenders?

                this.runningUpdates[i].OnUpdate(deltaTime);
            }
        }

        private void OnPlatformLateUpdate(object sender, System.EventArgs e)
        {
            if (this.isPaused)
            {
                return;
            }

            float deltaTime = Platform.GetDeltaTime();

            for (int i = 0; i < this.runningLateUpdates.Count; i++)
            {
                //// if this is a bad reference, then remove it (and swap with last one in the list)
                //// if not enabled, then skip it
                //// keep stats on worst offenders?

                this.runningLateUpdates[i].OnLateUpdate(deltaTime);
            }
        }

        private void OnPlatformFixedUpdate(object sender, System.EventArgs e)
        {
            if (this.isPaused)
            {
                return;
            }

            float fixedDeltaTime = Platform.GetPhysicsDeltaTime();

            for (int i = 0; i < this.runningFixedUpdates.Count; i++)
            {
                //// if this is a bad reference, then remove it (and swap with last one in the list)
                //// if not enabled, then skip it
                //// keep stats on worst offenders?

                this.runningFixedUpdates[i].OnFixedUpdate(fixedDeltaTime);
            }
        }

        private void ProcessActivationRequests()
        {
            //// TODO [bgish]: Need to handle any exception being thrown

            if (this.bootloader.IsBooted == false)
            {
                return;
            }

            var startTime = Platform.GetTimeSinceStartup();
            var endTime = startTime + (this.maxActivationTimeInMillis / 1000.0);
            var currentTime = startTime;

            while (currentTime < endTime)
            {
                if (objects.Count > 0)
                {
                    ProcessMonobehaviours();
                }
                else if (this.awakes.Count > 0)
                {
                    var awake = this.awakes.Dequeue();

                    if (awake != null)
                    {
                        awake.OnAwake(this.bootloader);
                    }
                }
                else if (this.starts.Count > 0)
                {
                    var start = this.starts.Dequeue();

                    if (start != null)
                    {
                        start.OnStart();
                    }
                }
                else if (this.updates.Count > 0)
                {
                    var update = this.updates.Dequeue();

                    if (update != null)
                    {
                        this.runningUpdates.Add(update);
                        this.updatesNeedReorder = true;
                    }
                }
                else if (this.lateUpdates.Count > 0)
                {
                    var lateUpdate = this.lateUpdates.Dequeue();

                    if (lateUpdate != null)
                    {
                        this.runningLateUpdates.Add(lateUpdate);
                    }
                }
                else if (this.fixedUpdates.Count > 0)
                {
                    var fixedUpdate = this.fixedUpdates.Dequeue();

                    if (fixedUpdate != null)
                    {
                        this.runningFixedUpdates.Add(fixedUpdate);
                    }
                }
                else
                {
                    isProcessing = false;
                    break;
                }

                currentTime = Platform.GetTimeSinceStartup();
            }

            void ProcessMonobehaviours()
            {
                if (objects.Count > 0)
                {
                    var monoBehaviour = objects.Dequeue();

                    if (monoBehaviour == null)
                    {
                        return;
                    }

                    if (monoBehaviour is IAwake awake)
                    {
                        this.awakes.Enqueue(awake);
                    }

                    if (monoBehaviour is IStart start)
                    {
                        this.starts.Enqueue(start);
                    }

                    if (monoBehaviour is IUpdate update)
                    {
                        this.updates.Enqueue(update);
                    }

                    if (monoBehaviour is ILateUpdate lateUpdate)
                    {
                        this.lateUpdates.Enqueue(lateUpdate);
                    }

                    if (monoBehaviour is IFixedUpdate fixedUpdate)
                    {
                        this.fixedUpdates.Enqueue(fixedUpdate);
                    }
                }
            }
        }

        protected override Task InitializeManager(Bootloader bootloader)
        {
            this.bootloader = bootloader;
            this.levelManager = this.bootloader.FindManager<LevelManager>();
            this.levelManager.AddLevelLoadPreprocessor(this);

            this.awakes = new(this.awakesInitialCapacity);
            this.starts = new(this.startsInitialCapacity);
            this.updates = new(this.updatesInitialCapacity);
            this.lateUpdates = new(this.lateUpdatesInitialCapacity);
            this.fixedUpdates = new(this.fixedUpdatesInitialCapacity);
            this.runningLateUpdates = new(this.runningLateUpdatesInitialCapacity);
            this.runningFixedUpdates = new(this.runningFixedUpdatesInitialCapacity);
            this.runningUpdates = new(this.runningUpdatesInitialCapacity);

            if (Platform.IsEditor || Platform.IsDebugBuild)
            {
                objects.OnGrow += () => Logger.LogWarning("ActivationManager monoBehaviours Queue Grew!");

                this.awakes.OnGrow += () => Logger.LogWarning("ActivationManager awakes Queue Grew!");
                this.starts.OnGrow += () => Logger.LogWarning("ActivationManager starts Queue Grew!");
                this.updates.OnGrow += () => Logger.LogWarning("ActivationManager updates Queue Grew!");
                this.lateUpdates.OnGrow += () => Logger.LogWarning("ActivationManager lateUpdates Queue Grew!");
                this.fixedUpdates.OnGrow += () => Logger.LogWarning("ActivationManager fixedUpdates Queue Grew!");
            }

            Platform.OnUpdate += this.OnPlatformUpdate;
            Platform.OnLateUpdate += this.OnPlatformLateUpdate;
            Platform.OnFixedUpdate += this.OnPlatformFixedUpdate;

            return Task.CompletedTask;
        }
    }
}
