using UnityEngine;
using UnityEngine.Assertions;
using System.Collections.Generic;
using Expression.Map;

namespace Expression.Map
{
    public class Hd2dAutoChipMeshFactory : Hd2dMeshFactory
    {
        private int unitPerWidth;
        private int unitPerHeight;

        public Hd2dAutoChipMeshFactory(int unitPerWidth, int unitPerHeight) : base()
        {
            this.unitPerWidth = unitPerWidth;
            this.unitPerHeight = unitPerHeight;
        }

        protected override Mesh CreateRectangle(Vector2Int offset)
        {
            var vartices = new Vector3[16] {
                new Vector2(-0.5f,0),
                new Vector2(-0.5f,0.5f),
                new Vector2(0,0),
                new Vector2(0,0.5f),

                new Vector2(0,0),
                new Vector2(0,0.5f),
                new Vector2(0.5f,0),
                new Vector2(0.5f,0.5f),

                new Vector2(-0.5f,-0.5f),
                new Vector2(-0.5f,0),
                new Vector3(0,-0.5f),
                new Vector2(0,0),

                new Vector3(0,-0.5f),
                new Vector2(0,0),
                new Vector3(0.5f,-0.5f),
                new Vector2(0.5f,0),
            };
            var triangles = new int[24] {
                0, 2, 1, 2, 1, 3,
                4, 6, 5, 6, 5, 7,
                8, 10, 9, 10, 9, 11,
                12, 14, 13, 14, 13, 15,
            };

            float xUnit = GetXUnit();
            float yUnit = GetYUnit();
            Vector2[] uvs = new Vector2[16];

            int leftUp = offset.x / 1000 % 10;
            int rightUp = offset.x / 100 % 10;
            int leftDown = offset.x / 10 % 10;
            int rightDown = offset.x / 1 % 10;

            uvs[0] = new Vector2(0, 1 - yUnit * (leftUp + 0.5f));
            uvs[1] = new Vector2(0, 1 - yUnit * leftUp);
            uvs[2] = new Vector2(0.5f, 1 - yUnit * (leftUp + 0.5f));
            uvs[3] = new Vector2(0.5f, 1 - yUnit * leftUp);

            uvs[4] = new Vector2(0.5f, 1 - yUnit * (rightUp + 0.5f));
            uvs[5] = new Vector2(0.5f, 1 - yUnit * rightUp);
            uvs[6] = new Vector2(1, 1 - yUnit * (rightUp + 0.5f));
            uvs[7] = new Vector2(1, 1 - yUnit * rightUp);

            uvs[8] = new Vector2(0, 1 - yUnit * (leftDown + 1));
            uvs[9] = new Vector2(0, 1 - yUnit * (leftDown + 0.5f));
            uvs[10] = new Vector2(0.5f, 1 - yUnit * (leftDown + 1));
            uvs[11] = new Vector2(0.5f, 1 - yUnit * (leftDown + 0.5f));

            uvs[12] = new Vector2(0.5f, 1 - yUnit * (rightDown + 1));
            uvs[13] = new Vector2(0.5f, 1 - yUnit * (rightDown + 0.5f));
            uvs[14] = new Vector2(1, 1 - yUnit * (rightDown + 1));
            uvs[15] = new Vector2(1, 1 - yUnit * (rightDown + 0.5f));

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
            return 1f / unitPerWidth;
        }

        private float GetYUnit()
        {
            return 1f / unitPerHeight;
        }
    }
}