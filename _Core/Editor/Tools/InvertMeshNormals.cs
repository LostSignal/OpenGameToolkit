//-----------------------------------------------------------------------
// <copyright file="InvertMeshNormals.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    public static class InvertMeshNormals
    {
        [MenuItem("CONTEXT/MeshFilter/Invert Normals")]
        public static void Invert(MenuCommand command)
        {
            var meshFilter = Selection.activeGameObject.GetComponent<MeshFilter>();
            var mesh = meshFilter.sharedMesh;

            var vertices = new Vector3[mesh.vertices.Length];
            var triangles = new int[mesh.triangles.Length];
            var normals = new Vector3[mesh.normals.Length];

            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = mesh.vertices[i];
            }

            for (int i = 0; i < triangles.Length; i += 3)
            {
                triangles[i + 0] = mesh.triangles[i + 2];
                triangles[i + 1] = mesh.triangles[i + 1];
                triangles[i + 2] = mesh.triangles[i + 0];
            }

            for (int i = 0; i < normals.Length; i++)
            {
                normals[i] = -mesh.normals[i];
            }

            var newMesh = new Mesh()
            {
                vertices = vertices,
                triangles = triangles,
                normals = normals,
                uv = mesh.uv,
                uv2 = mesh.uv2,
            };

            newMesh.RecalculateBounds();

            meshFilter.sharedMesh = newMesh;
        }
    }
}
