//-----------------------------------------------------------------------
// <copyright file="IPlayerProximityListener.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    public interface IPlayerProximityListener
    {
        void OnPlayerEnterProximity();

        void OnPlayerExitProximity();
    }
}
