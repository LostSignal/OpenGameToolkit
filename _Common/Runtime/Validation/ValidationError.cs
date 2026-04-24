//-----------------------------------------------------------------------
// <copyright file="ValidationError.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    public class ValidationError
    {
        public object AffectedObject { get; set; }

        public string AffectedObjectPath { get; set; }

        public string AffectedType { get; set; }

        public string Error { get; set; }

        public string Description { get; set; }
    }
}
