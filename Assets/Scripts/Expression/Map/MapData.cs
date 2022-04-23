using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Expression.Map
{
    public class MapData
    {
        public MapId Id { get; private set; }

        public Texture2D UnderTexture { get; private set; }

        public Texture2D UpperTexture { get; private set; }

        public int PixelPerUnit { get; private set; }

        public int Width { get; private set; }

        public int Height { get; private set; }

        public MovableInfo[,] MovableGrid { get; private set; }

        public MapEvent.EventData[] EventDataArray { get; private set; }

        public MapData(MapId id,Texture2D underTexture, Texture2D upperTexture, int width, int height,
         MovableInfo[,] movableGrid, MapEvent.EventData[] eventDataArray)
        {
            // テクスチャサイズの整合性チェック
            Assert.IsTrue(underTexture.width / width == underTexture.height / height
        && upperTexture.width / width == upperTexture.height / height
        && underTexture.width == upperTexture.width);

            Id = id;
            UnderTexture = underTexture;
            UpperTexture = upperTexture;
            PixelPerUnit = UnderTexture.width / width;
            Width = width;
            Height = height;
            MovableGrid = movableGrid;
            EventDataArray = eventDataArray;
        }
    }
}
