//-----------------------------------------------------------------------
// <copyright file="SecretString.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System;
    using UnityEngine;

    //// NOTE [bgish]: Only the key is ever serialized.  The actual value lives in the SecretsStore
    ////               (ProjectSettings/OGTSecrets.json) and is looked up on demand.
    [Serializable]
    public class SecretString
    {
        [SerializeField] private string secretKey;

        public SecretString()
        {
        }

        public SecretString(string secretKey)
        {
            this.secretKey = secretKey;
        }

        public string SecretKey => this.secretKey;

        public bool HasKey => string.IsNullOrEmpty(this.secretKey) == false;

        public bool HasValue => SecretsStore.Contains(this.secretKey);

        public string Value => SecretsStore.GetValue(this.secretKey);

        public bool TryGetValue(out string value)
        {
            return SecretsStore.TryGetValue(this.secretKey, out value);
        }

        // Never leak the secret through ToString (logging, string interpolation, etc)
        public override string ToString()
        {
            return this.HasKey ? $"SecretString({this.secretKey})" : "SecretString(None)";
        }
    }
}
