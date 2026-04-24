//-----------------------------------------------------------------------
// <copyright file="StringInputResult.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if UNITY

namespace OGT
{
    public struct StringInputResult
    {
        public InputResult Result;
        public string Text;

        public enum InputResult
        {
            Cancel,
            Ok,
        }
    }
}

#endif
