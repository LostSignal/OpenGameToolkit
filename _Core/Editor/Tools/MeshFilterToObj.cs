//-----------------------------------------------------------------------
// <copyright file="MeshFilterToObj.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if UNITY_EDITOR

namespace OGT
{
    using System.IO;
    using System.Linq;
    using System.Text;
    using UnityEditor;
    using UnityEngine;

    public static class MeshFilterToObj
    {
        public static void ExportMeshFilterToObjFile(MeshFilter meshFilter, string fullObjFilePath)
        {
            var outputFolder = Path.GetDirectoryName(fullObjFilePath);
            var objFileName = Path.GetFileName(fullObjFilePath);
            var objFileNameNoExtension = Path.GetFileNameWithoutExtension(fullObjFilePath);
            var objAssetPath = GetAssetPath(fullObjFilePath);

            // Write out the Obj file
            File.WriteAllText(fullObjFilePath, MeshFilterToString(meshFilter, objFileNameNoExtension));

            // Write out the materials file
            var materialNames = meshFilter.GetComponent<MeshRenderer>().sharedMaterials.Select(x => x.name).ToList();
            File.WriteAllText(Path.Combine(outputFolder, objFileNameNoExtension) + ".mtl", string.Concat(materialNames.Select(x => $"newmtl {x}\n")));

            // Import the Obj file
            AssetDatabase.Refresh();
            AssetDatabase.ImportAsset(objAssetPath);

            // Updating the Obj import settings and reimporting
            ModelImporter importer = (ModelImporter)ModelImporter.GetAtPath(objAssetPath);
            if (importer)
            {
                importer.importNormals = ModelImporterNormals.Calculate;
                importer.optimizeMeshPolygons = true;
                importer.optimizeMeshVertices = true;

                foreach (var material in meshFilter.GetComponent<MeshRenderer>().sharedMaterials)
                {
                    importer.AddRemap(new AssetImporter.SourceAssetIdentifier(material), material);
                }

                EditorUtility.SetDirty(importer);
                importer.SaveAndReimport();
            }
        }

        [MenuItem("CONTEXT/MeshFilter/Export To OBJ")]
        private static void ExportMeshFilter()
        {
            var gameObject = Selection.activeObject as GameObject;
            var meshFilter = gameObject.GetComponent<MeshFilter>();
            var savePath = EditorUtility.SaveFilePanel("Save OBJ", Path.GetDirectoryName(gameObject.scene.path), $"{gameObject.name}.obj", "obj");

            if (string.IsNullOrEmpty(savePath) == false)
            {
                ExportMeshFilterToObjFile(meshFilter, savePath);
            }
        }

        //// http://wiki.unity3d.com/index.php?title=ObjExporter
        private static string MeshFilterToString(MeshFilter meshFilter, string mtlFileNameNoExtension = null)
        {
            var stringBuilder = new StringBuilder();
            var materials = meshFilter.GetComponent<MeshRenderer>().sharedMaterials;
            var mesh = meshFilter.sharedMesh;

            // Writing out the materials file (if it exists)
            if (string.IsNullOrEmpty(mtlFileNameNoExtension) == false)
            {
                stringBuilder.Append($"mtllib ./{mtlFileNameNoExtension}.mtl\n");
            }

            stringBuilder.Append("g ").Append(meshFilter.name).Append("\n");

            foreach (Vector3 v in mesh.vertices)
            {
                // NOTE [bgish]: Flipping the X axis
                stringBuilder.Append(string.Format("v {0} {1} {2}\n", -v.x, v.y, v.z));
            }

            stringBuilder.Append("\n");

            foreach (Vector3 v in mesh.normals)
            {
                // NOTE [bgish]: Flipping the X axis
                stringBuilder.Append(string.Format("vn {0} {1} {2}\n", -v.x, v.y, v.z));
            }

            stringBuilder.Append("\n");

            foreach (Vector3 v in mesh.uv)
            {
                stringBuilder.Append(string.Format("vt {0} {1}\n", v.x, v.y));
            }

            for (int material = 0; material < mesh.subMeshCount; material++)
            {
                stringBuilder.Append("\n");
                stringBuilder.Append("usemtl ").Append(materials[material].name).Append("\n");
                stringBuilder.Append("usemap ").Append(materials[material].name).Append("\n");

                int[] triangles = mesh.GetTriangles(material);

                for (int i = 0; i < triangles.Length; i += 3)
                {
                    // NOTE [bgish]: Changing winding order
                    stringBuilder.Append(
                        string.Format(
                            "f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}\n",
                            triangles[i + 2] + 1,
                            triangles[i + 1] + 1,
                            triangles[i + 0] + 1));
                }
            }

            return stringBuilder.ToString();
        }

        private static string GetAssetPath(string fullPath)
        {
            string currentWorkingDirectory = Path.GetFullPath(".");
            return fullPath.Substring(currentWorkingDirectory.Length + 1).Replace("\\", "/");
        }
    }
}

#endif
