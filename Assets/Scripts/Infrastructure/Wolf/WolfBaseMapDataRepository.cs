using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using Expression.Map;
using Util;

namespace Infrastructure
{
    // 【暫定】BasemapDataのみを抜き出すように修正
    public class WolfBaseMapDataRepository : IBaseMapDataRepository
    {
        private string dirPath = $"{Application.streamingAssetsPath}/Data/MapData";
        private Dictionary<MapId, string> mapNameDict;

        private Dictionary<MapId, BaseMapData> mapDataDict;

        // 【暫定】エディタ層と名前を共有できるよう、共有文字列クラスを整備する
        private string tileFileKey = "Hd2dTileSetting";
        private const int CHIP_COUNT = 2500;// 仮で決め打ち
        private Hd2dTileInfoList tileInfoList;

        public WolfBaseMapDataRepository()
        {
            // 暫定：mpsをシステム変数DBから読み込めるようになるまで固定の値を割り当てておく
            string[] fileNames = { "Dungeon.mps", "SampleMapA.mps", "SampleMapB.mps", "TitleMap.mps" };
            mapNameDict = new Dictionary<MapId, string>();
            for (int i = 0; i < fileNames.Length; i++)
            {
                mapNameDict.Add(new MapId(i), $"{dirPath}/{fileNames[i]}");
            }
            mapDataDict = new Dictionary<MapId, BaseMapData>();

            LoadTileInfo();
        }

        public BaseMapData Find(MapId id)
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
                    WolfHd2dMapFactory creator = new WolfHd2dMapFactory(id);
                    mapDataDict.Add(id, creator.Create());
                    return mapDataDict[id];
                }
                else
                {
                    Assert.IsTrue(false, "Invalid map id was specified");
                    return null;
                }
            }
        }

        public int GetCount()
        {
            return mapDataDict.Count;
        }

        private void LoadTileInfo()
        {
            try
            {
                string json = PlayerPrefs.GetString(tileFileKey);
                tileInfoList = JsonUtility.FromJson<Hd2dTileInfoList>(json);
                Debug.Log("Loaded tile data");
            }
            catch
            {
                tileInfoList = new Hd2dTileInfoList(CHIP_COUNT);
                for (int i = 0; i < CHIP_COUNT; i++)
                {
                    var constraints = new Dictionary<Direction, Expression.Map.Hd2d.NeighborConstraint>();
                    constraints.Add(Direction.Up, new Expression.Map.Hd2d.NeighborConstraint(false, Vector3Int.zero));
                    constraints.Add(Direction.Right, new Expression.Map.Hd2d.NeighborConstraint(false, Vector3Int.zero));
                    constraints.Add(Direction.Down, new Expression.Map.Hd2d.NeighborConstraint(false, Vector3Int.zero));
                    constraints.Add(Direction.Left, new Expression.Map.Hd2d.NeighborConstraint(false, Vector3Int.zero));
                    tileInfoList[i] = new Hd2dTileInfo(Vector3.zero, MapBlockType.Cube, new Expression.Map.Hd2d.NeighborConstraintDict(constraints));
                }
                Debug.Log("Initialized tile info list");
            }
        }
    }
}
