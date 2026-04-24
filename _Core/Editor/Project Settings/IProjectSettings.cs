//-----------------------------------------------------------------------
// <copyright file="IProjectSettings.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    public interface IProjectSettings
    {
        string AssetName { get; }

        void Initialize();

        void Save();

        void LoadDefaults();
    }
}
