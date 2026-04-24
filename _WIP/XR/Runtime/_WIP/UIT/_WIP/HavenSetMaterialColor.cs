//-----------------------------------------------------------------------
// <copyright file="HavenSetMaterialColor.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.Haven
{
    using UnityEngine;

    //// NOTE [bgish]: This is a temporary class that will be replaced by lost actions.  Purely using this for testing purposes only
    public class HavenSetMaterialColor : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField] private Renderer rendererToSet;
        [SerializeField] private Color color;
#pragma warning restore 0649

        public void SetColor()
        {
            this.rendererToSet.material.color = this.color;
        }
    }
}
