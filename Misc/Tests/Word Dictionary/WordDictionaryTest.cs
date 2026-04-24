//-----------------------------------------------------------------------
// <copyright file="WordDictionaryTest.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using NUnit.Framework;
    using UnityEditor;

    public class WordDictionaryTest
    {
        [Test]
        public void Validate()
        {
            string dictionaryAssetPath = AssetDatabase.GUIDToAssetPath("09bf53d50e170ac4ea0f949dd001124a");

            Assert.False(string.IsNullOrWhiteSpace(dictionaryAssetPath), "Couldn't find \"English Dictionary\" to test with.");

            var dictionary = AssetDatabase.LoadAssetAtPath<WordDictionary>(dictionaryAssetPath);

            Assert.NotNull(dictionary);

            Assert.True(dictionary.IsValidWord("aa"));
            Assert.True(dictionary.IsValidWord("zzzs"));
            Assert.True(dictionary.IsValidWord("commendableness"));
            Assert.True(dictionary.IsValidWord("redistributions"));
            Assert.True(dictionary.IsValidWord("unbracketed"));

            Assert.False(dictionary.IsValidWord(string.Empty));
            Assert.False(dictionary.IsValidWord(null));
            Assert.False(dictionary.IsValidWord("unbowings"));
            Assert.False(dictionary.IsValidWord("brian"));
        }
    }
}
