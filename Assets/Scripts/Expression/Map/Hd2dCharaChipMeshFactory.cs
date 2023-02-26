using UnityEngine;
using UnityEngine.Assertions;
using System.Collections.Generic;
using Expression.Map;

namespace Expression.Map
{
    public class Hd2dCharaChipMeshFactory
    {
        private int unitPerWidth;
        private int unitPerHeight;

        public Hd2dCharaChipMeshFactory(int unitPerWidth, int unitPerHeight) : base()
        {
            this.unitPerWidth = unitPerWidth;
            this.unitPerHeight = unitPerHeight;
        }

        public Mesh Create(Direction direction,int pattern)
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
            Vector2Int offset = GetOffsetFromDirection(direction);
            offset += Vector2Int.right * pattern;
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

        private float GetXUnit()
        {
            return 1.0f / unitPerWidth;
        }

        private float GetYUnit()
        {
            return 1.0f / unitPerHeight;
        }

        private Vector2Int GetOffsetFromDirection(Direction direction)
        {
            switch (direction)
            {
                case Direction.Down:
                    return new Vector2Int(0, 0);
                case Direction.Left:
                    return new Vector2Int(0, 1);
                case Direction.Right:
                    return new Vector2Int(0, 2);
                case Direction.Up:
                    return new Vector2Int(0, 3);
                default:
                    return new Vector2Int(0, 0);
            }
        }
    }
}