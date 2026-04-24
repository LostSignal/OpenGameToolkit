//-----------------------------------------------------------------------
// <copyright file="AnonymousCloudFunctionAttribute.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if !UNITY || USING_PLAYFAB

namespace Lost.CloudFunctions
{
    public sealed class AnonymousCloudFunctionAttribute : System.Attribute
    {
        public AnonymousCloudFunctionAttribute(string category, string name)
        {
            this.Category = category;
            this.Name = name;
        }

        public string Category { get; private set; }

        public string Name { get; private set; }
    }
}

#endif
