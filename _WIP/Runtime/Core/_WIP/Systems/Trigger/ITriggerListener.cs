//-----------------------------------------------------------------------
// <copyright file="ITriggerListener.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    public interface ITriggerListener
    {
        void OnPlayerEnterTrigger();

        void OnPlayerExitTrigger();
    }
}
