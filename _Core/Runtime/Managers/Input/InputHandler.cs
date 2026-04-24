//-----------------------------------------------------------------------
// <copyright file="InputHandler.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System.Collections.Generic;

    public interface InputHandler
    {
        void HandleInputs(List<Input> touches, Input mouse, Input pen);
    }
}
