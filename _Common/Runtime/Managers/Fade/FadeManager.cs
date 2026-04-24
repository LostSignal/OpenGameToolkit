//-----------------------------------------------------------------------
// <copyright file="FadeManager.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System.Threading.Tasks;

    public class FadeManager : Manager
    {
        protected override Task InitializeManager(Bootloader bootloader)
        {
            return Task.CompletedTask;
        }
    }
}
