//-----------------------------------------------------------------------
// <copyright file="ValidationReport.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System.Collections.Generic;

    public class ValidationReport
    {
        private List<ValidationError> errors = new List<ValidationError>();

        public List<ValidationError> Errors => this.errors;
    }
}
