using UnityEngine;
using UnityEngine.Assertions;
using System.Collections.Generic;

namespace Expression.Map
{
    public abstract class ChipSelector
    {

        public ChipSelector()
        {
        }

        public Vector2[] GetUvs(Vector2Int offset, MeshType meshType)
        {
            switch (meshType)
            {
                case MeshType.Rectangle:
                    return GetSquareUvs(offset);
                case MeshType.LeftTriangle:
                    return GetLeftTriangleUvs(offset);
                case MeshType.RightTriangle:
                    return GetRightTriangleUvs(offset);
            }

            Assert.IsTrue(false, "不正なメッシュタイプが指定されました");
            return null;
        }

        protected abstract Vector2[] GetSquareUvs(Vector2Int offset);

        protected abstract Vector2[] GetLeftTriangleUvs(Vector2Int offset);

        protected abstract Vector2[] GetRightTriangleUvs(Vector2Int offset);
    }
}