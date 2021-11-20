using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Expression
{
    public class MapData
    {
        public Texture2D UnderTexture { get; private set; }

        public Texture2D UpperTexture { get; private set; }

        public int PixelPerUnit { get; private set; }

        public int Width { get; private set; }

        public int Height { get; private set; }

        public int[,] MovableGrid { get; private set; }

        public MapData(Texture2D underTexture, Texture2D upperTexture, int width, int height, int[,] movableGrid)
        {
            // テクスチャサイズの整合性チェック
            Assert.IsTrue(UnderTexture.width / width == UnderTexture.height / height
        && UpperTexture.width / width == UpperTexture.height / height
        && UnderTexture.width == UpperTexture.width);

            UnderTexture = underTexture;
            UpperTexture = upperTexture;
            PixelPerUnit = UnderTexture.width / width;
            Width = width;
            Height = height;
            MovableGrid = movableGrid;
        }
    }
}
