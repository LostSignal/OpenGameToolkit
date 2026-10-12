//-----------------------------------------------------------------------
// <copyright file="DevelopmentBuildOnly.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    public class DevelopmentBuildOnly : GameBehavior, IAwake
    {
        public void OnAwake(Bootloader bootloader)
        {
            if (RuntimeSettings.IsOgtDevelopmentBuild == false)
            {
                this.gameObject.SetActive(false);
                return;
            }
        }
    }
}
