//-----------------------------------------------------------------------
// <copyright file="ReleasesManager.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if UNITY

namespace OGT
{
    using System;
    using System.Collections;
    using System.Threading.Tasks;
    using UnityEngine;

    ////
    //// NEED TO MAKE A RELEASES APP CONFIG
    //// Will have the URL
    //// WIll have the blob storage upload info
    ////
    public sealed class ReleasesManager : Manager
    {
#pragma warning disable 0649
        [SerializeField] private StorageLocation storageLocation;
        [SerializeField] private Release hardCodedRelease;
#pragma warning restore 0649

        public enum StorageLocation
        {
            HardCoded,
            Url,
        }

        public Release CurrentRelease { get; private set; }

        protected override Task InitializeManager(Bootloader bootloader)
        {
            return Task.CompletedTask;
        }

        //// public override void Initialize()
        //// {
        ////     this.StartCoroutine(InitializeCoroutine());
        ////
        ////     IEnumerator InitializeCoroutine()
        ////     {
        ////         if (this.storageLocation == StorageLocation.HardCoded)
        ////         {
        ////             this.CurrentRelease = this.hardCodedRelease;
        ////         }
        ////         else if (this.storageLocation == StorageLocation.Url)
        ////         {
        ////             throw new NotImplementedException();
        ////         }
        ////         else
        ////         {
        ////             throw new Exception($"Unknown StorageLocation {this.storageLocation} Found!");
        ////         }
        ////
        ////         this.SetInstance(this);
        ////
        ////         yield break;
        ////     }
        //// }
        ////
        //// public Coroutine ShowForceUpdateDialog()
        //// {
        ////     return CoroutineRunner.Instance.StartCoroutine(Coroutine());
        ////
        ////     static IEnumerator Coroutine()
        ////     {
        ////         // TODO [bgish]: Implement
        ////         yield break;
        ////     }
        //// }
    }
}

#endif
