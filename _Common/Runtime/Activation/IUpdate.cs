//-----------------------------------------------------------------------
// <copyright file="IStart.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    public interface IUpdate
    {
        int UpdateOrder { get => 0; }

        void OnUpdate(float deltaTime);
    }
}
