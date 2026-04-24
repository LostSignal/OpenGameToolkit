//-----------------------------------------------------------------------
// <copyright file="BeginGridRowScope.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.EditorGrid
{
    using System;

    public class BeginGridRowScope : IDisposable
    {
        private readonly EditorGrid grid;

        public BeginGridRowScope(EditorGrid grid)
        {
            this.grid = grid;
            this.grid.BeginRow();
        }

        public void Dispose()
        {
            this.grid.EndRow();
        }
    }
}
