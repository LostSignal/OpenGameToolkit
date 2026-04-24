//-----------------------------------------------------------------------
// <copyright file="LoggingManager.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System.Threading.Tasks;

    ////
    //// NOTE [bgish]: One day this class will let me get all logging channels and
    ////               enable/disable them over UDP for debug purposes.
    ////
    public class LoggingManager : Manager
    {
        protected override Task InitializeManager(Bootloader bootloader)
        {
            return Task.CompletedTask;
        }
    }
}
