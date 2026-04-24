//-----------------------------------------------------------------------
// <copyright file="IValidate.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    public interface IValidate
    {
        void Validate(ValidationReport report, bool isSceneObject);
    }
}
