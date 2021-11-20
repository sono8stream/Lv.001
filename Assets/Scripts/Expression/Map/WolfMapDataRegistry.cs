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
    public class WolfMapDataRepository : IMapDataRepository
    {
        private string dirPath = "Assets/Resources/Data/MapData";
        private Dictionary<MapId, string> mapNameDict;

        private Dictionary<MapId, MapData> mapDataDict;

        public WolfMapDataRepository()
        {
            // 暫定：mpsをシステム変数DBから読み込めるようになるまでフォルダ全体のmpsを拾ってくる
            string[] filePaths = System.IO.Directory.GetFiles(dirPath, "*.mps");
            mapNameDict = new Dictionary<MapId, string>();
            for (int i = 0; i < filePaths.Length; i++)
            {
                mapNameDict.Add(new MapId(i), filePaths[i]);
            }
            mapDataDict = new Dictionary<MapId, MapData>();
        }

        public MapData Find(MapId id)
        {
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
