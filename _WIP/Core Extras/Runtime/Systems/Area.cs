//-----------------------------------------------------------------------
// <copyright file="Area.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if UNITY

namespace OGT
{
    using System;
    using UnityEngine;

    public enum AreaType
    {
        Sphere,
        Cylinder,
        Box,
    }

    [Serializable]
    public struct Area
    {
        public AreaType Type;
        public Vector3 Size;

        public bool IsInside(Matrix4x4 worldToLocalMatrix, Vector3 worldPosition)
        {
            var localPostiion = worldToLocalMatrix.MultiplyPoint(worldPosition);

            if (this.Type == AreaType.Sphere)
            {
                return localPostiion.sqrMagnitude < (this.Size.x * this.Size.x);
            }
            else if (this.Type == AreaType.Cylinder)
            {
                return localPostiion.sqrMagnitude < (this.Size.x * this.Size.x) &&
                    localPostiion.y < this.Size.y / 2.0f &&
                    localPostiion.y > -this.Size.y / 2.0f;
            }
            else if (this.Type == AreaType.Box)
            {
                return localPostiion.x < this.Size.x / 2.0f &&
                    localPostiion.x > -this.Size.x / 2.0f &&
                    localPostiion.y < this.Size.y / 2.0f &&
                    localPostiion.y > -this.Size.y / 2.0f &&
                    localPostiion.z < this.Size.z / 2.0f &&
                    localPostiion.z > -this.Size.z / 2.0f;
            }
            else
            {
                throw new NotImplementedException($"{nameof(Area)} IsInside encountered unknown {nameof(AreaType)} {this.Type.ToString()}");
            }
        }

        public void Draw(Transform transform, Area area, Color color)
        {
#if UNITY_EDITOR

            Gizmos.color = color;

            if (area.Type == AreaType.Sphere)
            {
                Gizmos.DrawWireSphere(transform.position, area.Size.x * transform.lossyScale.x);
            }
            else if (area.Type == AreaType.Cylinder)
            {
                GizmosUtil.DrawWireCylinder(transform, area.Size.x, area.Size.y);
            }
            else if (area.Type == AreaType.Box)
            {
                GizmosUtil.DrawWireCube(transform, area.Size);
            }

#endif
        }
    }
}

#endif
