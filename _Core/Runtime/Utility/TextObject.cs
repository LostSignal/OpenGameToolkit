//-----------------------------------------------------------------------
// <copyright file="TextObject.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using UnityEngine;

    [CreateAssetMenu(menuName = "Lost/Text Object")]
    public class TextObject : ScriptableObject
    {
        [SerializeField] private string text;

        public string Text
        {
            get => this.text;
            set => this.text = value;
        }
    }
}
