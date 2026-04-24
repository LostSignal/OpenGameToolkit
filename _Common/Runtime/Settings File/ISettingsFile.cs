//-----------------------------------------------------------------------
// <copyright file="ISettingsFile.cs" company="Lost Signal">
//     Copyright (c) Lost Signal. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    public interface ISettingsFile
    {
        string ParentId { get; }

        string Id { get; }

        string Name { get; set; }

        bool IsSelectable { get; set; }

        string Content { get; set; }
    }
}
