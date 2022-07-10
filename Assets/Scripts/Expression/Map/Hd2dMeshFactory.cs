using UnityEngine;
using UnityEngine.Assertions;
using System.Collections.Generic;
using Expression.Map;

namespace Expression.Map
{
    public abstract class Hd2dMeshFactory
    {
        public Hd2dMeshFactory()
        {
        }

        public Mesh CreateMesh(MeshType meshType, Vector2Int uvChipOffset)
        {
            switch (meshType)
            {
                case MeshType.Rectangle:
                    return CreateRectangle(uvChipOffset);
                case MeshType.LeftTriangle:
                    return CreateLeftTriangle(uvChipOffset);
                case MeshType.RightTriangle:
                    return CreateRightTriangle(uvChipOffset);
                default:
                    throw new System.Exception("Undefined mesh type was specified!");
            }
        }

        protected abstract Mesh CreateRectangle(Vector2Int uvChipOffset);

        protected abstract Mesh CreateLeftTriangle(Vector2Int uvChipOffset);

        protected abstract Mesh CreateRightTriangle(Vector2Int uvChipOffset);
    }
}