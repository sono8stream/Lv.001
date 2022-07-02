using UnityEngine;
using UnityEngine.Assertions;
using System.Collections.Generic;

namespace Hd2d
{
    public class MeshFactory
    {
        public MeshFactory()
        {
        }

        public Mesh CreateMesh(MeshType meshType, Vector2Int uvChipOffset)
        {
            Mesh mesh = new Mesh();
            var selector = new ChipSelector(8, 256);
            switch (meshType)
            {
                case MeshType.Rectangle:
                    {
                        var vartices = new Vector3[4] {
                new Vector2(-0.5f,-0.5f),
                new Vector2(-0.5f,0.5f),
                new Vector2(0.5f,-0.5f),
                new Vector2(0.5f,0.5f)
            };
                        mesh.SetVertices(vartices);
                        var triangles = new int[6] { 0, 1, 2, 2, 1, 3 };
                        mesh.SetTriangles(triangles, 0);
                        mesh.SetUVs(0, selector.GetSquareUvs(uvChipOffset));
                    }
                    break;
                case MeshType.Triangle:
                    {
                        var vartices = new Vector3[3] {
                new Vector2(-0.5f,-0.5f),
                new Vector2(-0.5f,0.5f),
                new Vector2(0.5f,-0.5f)
            };
                        mesh.SetVertices(vartices);
                        var triangles = new int[3] { 0, 1, 2 };
                        mesh.SetTriangles(triangles, 0);
                        mesh.SetUVs(0, selector.GetTriUvs(uvChipOffset));
                    }
                    break;
            }

            return mesh;
        }
    }
}