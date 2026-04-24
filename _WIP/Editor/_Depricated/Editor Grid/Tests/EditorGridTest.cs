//-----------------------------------------------------------------------
// <copyright file="EditorGridTest.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.EditorGrid
{
    using UnityEditor;
    using UnityEngine;

    //// [CustomEditor(typeof(GridTestObject))]
    public class EditorGridTest : Editor
    {
        private const int ColumnCount = 10;

        private static readonly EditorGrid Grid;

        // Use this for initialization
        static EditorGridTest()
        {
            var gridDefinition = new EditorGridDefinition();

            for (int i = 0; i < ColumnCount; i++)
            {
                gridDefinition.AddColumn("Column " + (i + 1), 100);
            }

            Grid = new EditorGrid(gridDefinition);
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            using (new BeginGridScope(Grid))
            {
                using (new BeginGridRowScope(Grid))
                {
                    for (int i = 0; i < ColumnCount; i++)
                    {
                        Grid.DrawFloat(i + 1.0f);
                    }
                }

                using (new BeginGridRowScope(Grid))
                {
                    for (int i = 0; i < ColumnCount; i++)
                    {
                        Grid.DrawInt(i + 1);
                    }
                }

                using (new BeginGridRowScope(Grid))
                {
                    for (int i = 0; i < ColumnCount; i++)
                    {
                        Grid.DrawBool(i % 2 == 0);
                    }
                }

                using (new BeginGridRowScope(Grid))
                {
                    for (int i = 0; i < ColumnCount; i++)
                    {
                        Grid.DrawColor(Color.white);
                    }
                }

                using (new BeginGridRowScope(Grid))
                {
                    for (int i = 0; i < ColumnCount; i++)
                    {
                        Grid.DrawLabel("Label " + (i + 1));
                    }
                }

                using (new BeginGridRowScope(Grid))
                {
                    for (int i = 0; i < ColumnCount; i++)
                    {
                        Grid.DrawObject(null, true);
                    }
                }

                using (new BeginGridRowScope(Grid))
                {
                    for (int i = 0; i < ColumnCount; i++)
                    {
                        Grid.DrawSprite(null, true);
                    }
                }

                using (new BeginGridRowScope(Grid))
                {
                    for (int i = 0; i < ColumnCount; i++)
                    {
                        Grid.DrawString("String " + (i + 1));
                    }
                }

                using (new BeginGridRowScope(Grid))
                {
                    for (int i = 0; i < ColumnCount; i++)
                    {
                        Grid.DrawVector3(new Vector3(i + 1, i + 1, i + 1));
                    }
                }
            }
        }
    }
}
