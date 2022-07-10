using UnityEngine;
using UnityEngine.Assertions;
using System.Collections.Generic;
using Expression.Map;

namespace Expression.Map
{
    public class Hd2dBaseChipMeshFactory : Hd2dMeshFactory
    {
        private int unitPerWidth;
        private int unitPerHeight;

        public Hd2dBaseChipMeshFactory(int unitPerWidth, int unitPerHeight) : base()
        {
            this.unitPerWidth = unitPerWidth;
            this.unitPerHeight = unitPerHeight;
        }

        protected override Mesh CreateRectangle(Vector2Int offset)
        {
            var vartices = new Vector3[4] {
                new Vector2(-0.5f,-0.5f),
                new Vector2(-0.5f,0.5f),
                new Vector2(0.5f,-0.5f),
                new Vector2(0.5f,0.5f)
            };
            var triangles = new int[6] { 0, 1, 2, 2, 1, 3 };

            float xUnit = GetXUnit();
            float yUnit = GetYUnit();
            Vector2[] uvs = new Vector2[4];
            uvs[0] = new Vector2(xUnit * offset.x, 1 - yUnit * (offset.y + 1));
            uvs[1] = new Vector2(xUnit * offset.x, 1 - yUnit * offset.y);
            uvs[2] = new Vector2(xUnit * (offset.x + 1), 1 - yUnit * (offset.y + 1));
            uvs[3] = new Vector2(xUnit * (offset.x + 1), 1 - yUnit * offset.y);

            Mesh mesh = new Mesh();
            mesh.SetVertices(vartices);
            mesh.SetTriangles(triangles, 0);
            mesh.SetUVs(0, uvs);
            return mesh;
        }

        protected override Mesh CreateLeftTriangle(Vector2Int offset)
        {
            var vartices = new Vector3[3] {
                new Vector2(-0.5f,-0.5f),
                new Vector2(-0.5f,0.5f),
                new Vector2(0.5f,-0.5f)
            };
            var triangles = new int[3] { 0, 1, 2 };

            float xUnit = GetXUnit();
            float yUnit = GetYUnit();
            Vector2[] uvs = new Vector2[3];
            uvs[0] = new Vector2(xUnit * offset.x, 1 - yUnit * (offset.y + 1));
            uvs[1] = new Vector2(xUnit * offset.x, 1 - yUnit * offset.y);
            uvs[2] = new Vector2(xUnit * (offset.x + 1), 1 - yUnit * (offset.y + 1));

            Mesh mesh = new Mesh();
            mesh.SetVertices(vartices);
            mesh.SetTriangles(triangles, 0);
            mesh.SetUVs(0, uvs);
            return mesh;
        }

        protected override Mesh CreateRightTriangle(Vector2Int offset)
        {
            var vartices = new Vector3[3] {
                new Vector2(-0.5f,-0.5f),
                new Vector2(0.5f,0.5f),
                new Vector2(0.5f,-0.5f)
            };
            var triangles = new int[3] { 0, 1, 2 };

            float xUnit = GetXUnit();
            float yUnit = GetYUnit();
            Vector2[] uvs = new Vector2[3];
            uvs[0] = new Vector2(xUnit * offset.x, 1 - yUnit * (offset.y + 1));
            uvs[1] = new Vector2(xUnit * (offset.x + 1), 1 - yUnit * offset.y);
            uvs[2] = new Vector2(xUnit * (offset.x + 1), 1 - yUnit * (offset.y + 1));

            Mesh mesh = new Mesh();
            mesh.SetVertices(vartices);
            mesh.SetTriangles(triangles, 0);
            mesh.SetUVs(0, uvs);
            return mesh;
        }

        private float GetXUnit()
        {
            return 1.0f / unitPerWidth;
        }

        private float GetYUnit()
        {
            return 1.0f / unitPerHeight;
        }
    }
}