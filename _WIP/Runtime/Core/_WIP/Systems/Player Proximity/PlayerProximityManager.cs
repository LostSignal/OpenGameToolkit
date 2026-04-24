//-----------------------------------------------------------------------
// <copyright file="PlayerProximityManager.cs" company="Lost Signal">
//     Copyright (c) Lost Signal. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if UNITY

namespace OGT
{
    using System.Threading.Tasks;
    using UnityEngine;

    public sealed class PlayerProximityManager : Manager, IUpdate
    {
#pragma warning disable 0649
        [SerializeField] private float runAllOverXSeconds;
#pragma warning restore 0649

        private PlayerProximityList playerProximityList;
        public int UpdateOrder => 1;

        public override void ResetToDefaults()
        {
            this.runAllOverXSeconds = 0.5f;
        }

        protected override Task InitializeManager(Bootloader bootloader)
        {
            var actorManager = bootloader.FindManager<ActorManager>();
            this.playerProximityList = new PlayerProximityList("Player Proximity List", 1000, actorManager);

            return Task.CompletedTask;
        }

        public void Register(PlayerProximity playerProximity)
        {
            Transform playerProximityTransform = playerProximity.ProximityTransform;

            this.playerProximityList.Add(
                playerProximity.GetEntityId(),
                new PlayerProximityItem
                {
                    WorldToLocal = playerProximityTransform.worldToLocalMatrix,
                    Area = playerProximity.Area,
                    IsDynamic = playerProximity.IsDynamic,
                    PlayerProximity = playerProximity,
                    Transform = playerProximityTransform,
                    IsInProximity = false,
                },
                playerProximity);
        }

        public void Unregister(PlayerProximity playerProximity)
        {
            this.playerProximityList.Remove(playerProximity.GetEntityId());
        }

        public void OnUpdate(float deltaTime)
        {
            this.playerProximityList.RunAllOverXSeconds(deltaTime, this.runAllOverXSeconds);
        }
    }
}

#endif
