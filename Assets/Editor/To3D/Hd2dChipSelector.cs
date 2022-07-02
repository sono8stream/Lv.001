using UnityEngine;
using UnityEngine.Assertions;
using System.Collections.Generic;

namespace Hd2d
{
    public class ChipSelector
    {
        private int unitPerWidth;
        private int unitPerHeight;

        public ChipSelector(int unitPerWidth, int unitPerHeight)
        {
            this.unitPerWidth = unitPerWidth;
            this.unitPerHeight = unitPerHeight;
        }

        public Vector2[] GetUvs(Vector2Int offset, MeshType meshType)
        {
            switch (meshType)
            {
                case MeshType.Rectangle:
                    return GetSquareUvs(offset);
                case MeshType.Triangle:
                    return GetTriUvs(offset);
            }

            Assert.IsTrue(false, "不正なメッシュタイプが指定されました");
            return null;
        }

        public Vector2[] GetSquareUvs(Vector2Int offset)
        {
            Vector2[] res = new Vector2[4];
            float xUnit = GetXUnit();
            float yUnit = GetYUnit();
            res[0] = new Vector2(xUnit * offset.x, 1 - yUnit * (offset.y + 1));
            res[1] = new Vector2(xUnit * (offset.x + 1), 1 - yUnit * (offset.y + 1));
            res[2] = new Vector2(xUnit * offset.x, 1 - yUnit * offset.y);
            res[3] = new Vector2(xUnit * (offset.x + 1), 1 - yUnit * offset.y);
            return res;
        }

        public Vector2[] GetTriUvs(Vector2Int offset)
        {
            Vector2[] res = new Vector2[3];
            float xUnit = GetXUnit();
            float yUnit = GetYUnit();
            res[0] = new Vector2(xUnit * offset.x, 1 - yUnit * (offset.y + 1));
            res[1] = new Vector2(xUnit * offset.x, 1 - yUnit * offset.y);
            res[2] = new Vector2(xUnit * (offset.x + 1), 1 - yUnit * (offset.y + 1));
            return res;
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