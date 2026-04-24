//-----------------------------------------------------------------------
// <copyright file="BeginGridScope.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.EditorGrid
{
    using System;

    public sealed class BeginGridScope : IDisposable
    {
        private readonly EditorGrid grid;

        public BeginGridScope(EditorGrid grid)
        {
            this.grid = grid;
            this.grid.BeginGrid();
        }

        public void Dispose()
        {
            this.grid.EndGrid();
        }
    }
}
