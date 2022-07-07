using UnityEngine;
using UnityEngine.Assertions;
using System.Collections.Generic;
using Expression.Map;

namespace Expression.Map
{
    public class MeshFactory
    {
        public MeshFactory()
        {
        }

        public Mesh CreateMesh(MeshType meshType, Vector2Int uvChipOffset)
        {
            Mesh mesh = new Mesh();
            var selector = new BaseChipSelector(8, 256);
            Vector3[] vartices = null;
            int[] triangles = null;

            switch (meshType)
            {
                case MeshType.Rectangle:
                    {
                        vartices = new Vector3[4] {
                new Vector2(-0.5f,-0.5f),
                new Vector2(-0.5f,0.5f),
                new Vector2(0.5f,-0.5f),
                new Vector2(0.5f,0.5f)
            };
                        triangles = new int[6] { 0, 1, 2, 2, 1, 3 };
                    }
                    break;
                case MeshType.LeftTriangle:
                    {
                        vartices = new Vector3[3] {
                new Vector2(-0.5f,-0.5f),
                new Vector2(-0.5f,0.5f),
                new Vector2(0.5f,-0.5f)
            };
                        triangles = new int[3] { 0, 1, 2 };
                    }
                    break;
                case MeshType.RightTriangle:
                    {
                        vartices = new Vector3[3] {
                new Vector2(-0.5f,-0.5f),
                new Vector2(0.5f,0.5f),
                new Vector2(0.5f,-0.5f)
            };
                        triangles = new int[3] { 0, 1, 2 };
                    }
                    break;
            }

            mesh.SetVertices(vartices);
            mesh.SetTriangles(triangles, 0);
            mesh.SetUVs(0, selector.GetUvs(uvChipOffset, meshType));

            return mesh;
        }
    }
}