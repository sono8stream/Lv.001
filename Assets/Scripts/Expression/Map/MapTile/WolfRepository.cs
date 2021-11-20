using System;
using UnityEngine.Assertions;
using Util.Wolf;

namespace Expression.Map.MapTile
{
    class WolfRepository
    {
        // 【暫定】ファイルパスをDIで指定
        private string dataPath = "Assets/Resources/Data/BasicData/TileSetData.dat";

        private static TileData[] dataArray;

        public TileData Find(int index)
        {
            if (dataArray == null)
            {
                LoadAllMapTilesFromDataBinary();
            }

            if (index < 0 && index >= dataArray.Length)
            {
                Assert.IsTrue(false);
                return null;
            }

            return dataArray[index];
        }

        private void LoadAllMapTilesFromDataBinary()
        {
            WolfDataReader reader = new WolfDataReader(dataPath);
            int settingCount = reader.ReadInt(0x0b, true, out int tmpOffset);
            dataArray = new TileData[settingCount];

            int autoTileCount = 15;
            int offset = 0x0f;
            for (int i = 0; i < settingCount; i++)
            {
                string settingName = reader.ReadString(offset, out offset);

                string baseTileFilePath = reader.ReadString(offset, out offset);

                string[] autoTileFilePaths = new string[15];
                for (int j = 0; j < autoTileCount; j++)
                {
                    autoTileFilePaths[j] = reader.ReadString(offset, out offset);
                }

                offset++;
                int unitTagLength = reader.ReadInt(offset, true, out offset);
                int[] unitTagNums = new int[unitTagLength];

                for (int j = 0; j < unitTagLength; j++)
                {
                    unitTagNums[j] = reader.ReadByte(offset, out offset);
                }

                offset++;
                int unitConfigLength = reader.ReadInt(offset, true, out offset);
                UnitTile[] unitTileConfigs = new UnitTile[unitConfigLength];

                for (int j = 0; j < unitConfigLength; j++)
                {
                    unitTileConfigs[j] = ReadUnitTile(reader, unitTagNums[j], offset, out offset);
                }

                dataArray[i] = new TileData(settingName, baseTileFilePath, autoTileFilePaths, unitTileConfigs);
            }
        }

        private UnitTile ReadUnitTile(WolfDataReader reader, int tagNumber, int offset, out int nextOffset)
        {
            int val = reader.ReadInt(offset, true, out nextOffset);

            return new UnitTile(0, null, false, tagNumber);
        }
    }
}