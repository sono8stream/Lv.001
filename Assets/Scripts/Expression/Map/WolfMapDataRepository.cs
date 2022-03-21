using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using Expression;
using Util;

namespace Expression.Map
{
    public class WolfMapDataRepository : Infrastructure.IMapDataRepository
    {
        private string dirPath = $"{Application.streamingAssetsPath}/Data/MapData";
        private Dictionary<MapId, string> mapNameDict;

        private Dictionary<MapId, MapData> mapDataDict;

        public WolfMapDataRepository()
        {
            // 暫定：mpsをシステム変数DBから読み込めるようになるまで固定の値を割り当てておく
            string[] fileNames = { "Dungeon.mps", "SampleMapA.mps", "SampleMapB.mps", "TitleMap.mps" };
            //string[] filePaths = Directory.GetFiles(dirPath, "*.mps");
            mapNameDict = new Dictionary<MapId, string>();
            for (int i = 0; i < fileNames.Length; i++)
            {
                mapNameDict.Add(new MapId(i), $"{dirPath}/{fileNames[i]}");
            }
            mapDataDict = new Dictionary<MapId, MapData>();
        }

        public MapData Find(MapId id)
        {
            // 【暫定】デバッグのために毎回読み出す
            if (mapDataDict.ContainsKey(id))
            {
                mapDataDict.Remove(id);
            }

            if (mapDataDict.ContainsKey(id))
            {
                return mapDataDict[id];
            }
            else
            {
                if (mapNameDict.ContainsKey(id))
                {
                    WolfMapCreator creator = new WolfMapCreator();
                    mapDataDict.Add(id, creator.Create(mapNameDict[id]));
                    return mapDataDict[id];
                }
                else
                {
                    Assert.IsTrue(false, "Invalid map id was specified");
                    return null;
                }
            }
        }
    }
}
