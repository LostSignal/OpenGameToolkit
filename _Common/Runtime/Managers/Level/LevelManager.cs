//-----------------------------------------------------------------------
// <copyright file="LevelManager.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public sealed class LevelManager : Manager
    {
        private readonly List<ILevelLoadPreprocessor> preprocessors = new();
        private readonly List<ILevelLoadPostprocessor> postprocessors = new();

        public void AddLevelLoadPreprocessor(ILevelLoadPreprocessor preprocessor)
        {
            this.preprocessors.Add(preprocessor);
        }

        public void AddLevelLoadPostprocessor(ILevelLoadPostprocessor postprocessor)
        {
            this.postprocessors.Add(postprocessor);
        }

        public void RemoveLevelLoadPreprocessor(ILevelLoadPreprocessor preprocessor)
        {
            this.preprocessors.Remove(preprocessor);
        }

        public void RemoveLevelLoadPostprocessor(ILevelLoadPostprocessor postprocessor)
        {
            this.postprocessors.Remove(postprocessor);
        }

        protected override Task InitializeManager(Bootloader bootloader)
        {
            return Task.CompletedTask;
        }

        /*
        * Make ActivationManager
          * Scans the entier scene for components with IAwake and IStart and process

        * FadeInOutManager

        * Level Load Manager
          * FadeInOutManager.FadeDown
            * Optional, Load Safe Space and Fade Up
          * Load the Level (shoudl have progress bar)
          * Wait for ActivationManager to finish
            * Objects with ILevelLoad interface register themselves
            *
          * Wait for LevelLoad.ILevelLoad interfaces to finsih
          * Fade Down, Teleport, Fade Up


        * ActivationManager, LevelManager, ILevelPreload, ILevelPostload
        * FadeManager
        * LostLODGroup inherits PlayerProximity
        public interface ILevelLoadProcessor
        {
            bool IsProcessing(float deltaTime);
        }

        LevelManager
            Warp(WarpType)  // FadeDown(), OnFadedDown?.Invoke(), Teleport(Vector3), FadeUp()
            IsLevelLoading = true
            ActivationManager.Awake
            ActivationManager.Start
            ActivationMangaer.LevelLoadProcessors
            IsLevelLading = false
            Warp(WarpType)  // FadeDown(), OnFadedDown?.Invoke(), Teleport(Vector3), FadeUp()

            * Work on LevelManager and ActivationManager systems (apart of Core or Common?)
            * Update all code to use those systems
            * Make IProcessBeforeLevelStart interface

        * What is the best way to hook into the LevelManager and do a ton of game specific work before Fading Up...
            * Have a scriptable object full of level settings (rig state, available weapons, etc), and it implements an interface bool IRun().  It returns true it's done, and false when it's not.  Then LevelManager has a RegisterWork?
            * Maybe it's better to have LevelManager.RegisterReady, and the interface is IReady, with property bool IsReady { get; }.  That way anything can register itself with the LevelManager in IAwake or IStart and as long as it's not ready, Level Manager won't finish and Fade Up.  Under the hood, the class could use any method for running the code over time.  Coroutine or WorkManager.
            * LevelManager
            * void RegisterLevelLoadWorker(ILevelLoadWorker)
            * ILevelLoadWorker
            * void LevelLoadStarted()
            * void LevelLoadComplete()
            * bool IsDone { get; }

        */

        //// * All level loads should not activate, and LevelManager should activate them manually and report stats for how long that takes.
        //// * SceneQueue
        //// * IsLevelLoaded Event
        ////   *  SceneQueue.Count == 0, LoadBalancer.IsBusy == false && SceneManager.IsLevelLoading == false
        ////
        //// * Takes a Level Asset that has a main LazyScene and a list of chunk scenes
        //// * Also has a Initial Chunks list
        //// * Level Load Process (should be an event fired so a Visual Scripting Graph can do all this
        ////   * Fades Down
        ////   * CollectGarbage
        ////   * Fades Up to Loading Environment (like Altspace/VR Chat)
        ////   * Unloads Previous Level
        ////   * Downloads Next Level (if applicable)
        ////   * Loads Next Level
        ////   * Waits for LoadBallancer.IsBusy to be false
        ////   * Fades Down
        ////   * CollectGarbage
        ////   * Fades up to new Level based on LevelStart script
        //// * LevelManager can manipulate the LoadBalancer?
        ////   * MaxMillsecondsPerFrame (in milliseconds)
        ////     * When doing a level load, it's higher than doing a hard load
        ////
        //// * Level Manager won't load levels/chunks untill Awake/Start Managers have completed?
        ////   * Load Level -> Wait for all managers to finish ->
        ////
        //// * Should we have a "loading" layer, and during level load we only render stuff on that layer?
        ////

        //// Full Fade Down = Fade Down + Incremetnal GC + Unload Unused Assets

        //// * LevelManager
        ////   * LoadLevel(blah, skipStagging = false)
        ////   * If Editor
        ////     * Instant Fade Down
        ////     * Wait for Managers
        ////     * Run Level Load Requested with currently active scene as the arguments
        ////       * It won't actually load it since it's already loaded
        ////
        //// ---
        ////
        //// Level Load Requested
        ////   Full Fade Down
        ////   Turn on Loading Area
        ////   Teleport to Loading Area
        ////   Fade Up
        ////   On Complete => Level Transition Begin
        ////
        //// Level Load Begin
        ////   Download
        ////   Load Level
        ////   Activate Level
        ////   Wait for OnManagersReady To Complete?  Or just till there are no load balancers done?
        ////     Some system will need to load chuncks depending on the players position, what will they hook into to load the chuncks?
        ////     The chunk system will need to look at the Starting position to figure out what chunks to load
        ////   Full Fade Down
        ////   Teleport to starting posotion
        ////   Fade Up
        ////   On Complete => Level Transition Finish
        ////
        //// Level Load Finish
        ////   ??? (AI Start?)
        // }
    }
}
