//-----------------------------------------------------------------------
// <copyright file="IAwake.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    public interface IAwake
    {
        void OnAwake(Bootloader bootloader);
    }
}
