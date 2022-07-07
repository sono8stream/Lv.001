using UnityEngine;
using UnityEngine.Assertions;
using System.Collections.Generic;

namespace Expression.Map
{
    public class BaseChipSelector : ChipSelector
    {
        private int unitPerWidth;
        private int unitPerHeight;

        public BaseChipSelector(int unitPerWidth, int unitPerHeight) : base()
        {
            this.unitPerWidth = unitPerWidth;
            this.unitPerHeight = unitPerHeight;
        }

        protected override Vector2[] GetSquareUvs(Vector2Int offset)
        {
            Vector2[] res = new Vector2[4];
            float xUnit = GetXUnit();
            float yUnit = GetYUnit();
            res[0] = new Vector2(xUnit * offset.x, 1 - yUnit * (offset.y + 1));
            res[1] = new Vector2(xUnit * offset.x, 1 - yUnit * offset.y);
            res[2] = new Vector2(xUnit * (offset.x + 1), 1 - yUnit * (offset.y + 1));
            res[3] = new Vector2(xUnit * (offset.x + 1), 1 - yUnit * offset.y);
            return res;
        }

        protected override Vector2[] GetLeftTriangleUvs(Vector2Int offset)
        {
            Vector2[] res = new Vector2[3];
            float xUnit = GetXUnit();
            float yUnit = GetYUnit();
            res[0] = new Vector2(xUnit * offset.x, 1 - yUnit * (offset.y + 1));
            res[1] = new Vector2(xUnit * offset.x, 1 - yUnit * offset.y);
            res[2] = new Vector2(xUnit * (offset.x + 1), 1 - yUnit * (offset.y + 1));
            return res;
        }

        protected override Vector2[] GetRightTriangleUvs(Vector2Int offset)
        {
            Vector2[] res = new Vector2[3];
            float xUnit = GetXUnit();
            float yUnit = GetYUnit();
            res[0] = new Vector2(xUnit * offset.x, 1 - yUnit * (offset.y + 1));
            res[1] = new Vector2(xUnit * (offset.x + 1), 1 - yUnit * offset.y);
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