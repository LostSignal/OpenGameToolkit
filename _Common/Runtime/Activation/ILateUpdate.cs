//-----------------------------------------------------------------------
// <copyright file="ILateUpdate.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    public interface ILateUpdate
    {
        void OnLateUpdate(float deltaTime);
    }
}
