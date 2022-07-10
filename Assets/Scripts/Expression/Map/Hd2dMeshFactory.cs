using UnityEngine;
using UnityEngine.Assertions;
using System.Collections.Generic;
using Expression.Map;

namespace Expression.Map
{
    public class Hd2dMeshFactory
    {
        public ChipSelector Selector { get; private set; }

        public Hd2dMeshFactory(ChipSelector selector)
        {
            this.Selector = selector;
        }

        public Mesh CreateMesh(MeshType meshType, Vector2Int uvChipOffset)
        {
            Mesh mesh = new Mesh();
            Vector3[] vartices = null;
            int[] triangles = null;

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

        public Mesh CreateRectangle(Vector2Int uvChipOffset)
        {
            var vartices = new Vector3[4] {
                new Vector2(-0.5f,-0.5f),
                new Vector2(-0.5f,0.5f),
                new Vector2(0.5f,-0.5f),
                new Vector2(0.5f,0.5f)
            };
            var triangles = new int[6] { 0, 1, 2, 2, 1, 3 };

            Mesh mesh = new Mesh();
            mesh.SetVertices(vartices);
            mesh.SetTriangles(triangles, 0);
            mesh.SetUVs(0, Selector.GetUvs(uvChipOffset, MeshType.Rectangle));
            return mesh;
        }

        public Mesh CreateLeftTriangle(Vector2Int uvChipOffset)
        {
            var vartices = new Vector3[3] {
                new Vector2(-0.5f,-0.5f),
                new Vector2(-0.5f,0.5f),
                new Vector2(0.5f,-0.5f)
            };
            var triangles = new int[3] { 0, 1, 2 };

            Mesh mesh = new Mesh();
            mesh.SetVertices(vartices);
            mesh.SetTriangles(triangles, 0);
            mesh.SetUVs(0, Selector.GetUvs(uvChipOffset, MeshType.LeftTriangle));
            return mesh;
        }

        public Mesh CreateRightTriangle(Vector2Int uvChipOffset)
        {
            var vartices = new Vector3[3] {
                new Vector2(-0.5f,-0.5f),
                new Vector2(0.5f,0.5f),
                new Vector2(0.5f,-0.5f)
            };
            var triangles = new int[3] { 0, 1, 2 };

            Mesh mesh = new Mesh();
            mesh.SetVertices(vartices);
            mesh.SetTriangles(triangles, 0);
            mesh.SetUVs(0, Selector.GetUvs(uvChipOffset, MeshType.RightTriangle));
            return mesh;
        }
    }
}