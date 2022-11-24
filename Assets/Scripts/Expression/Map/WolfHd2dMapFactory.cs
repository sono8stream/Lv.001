using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Expression.Map
{
    public class WolfHd2dMapFactory : WolfBaseMapFactory
    {
        private Hd2dTileInfoList tileInfoList;
        private Shader shader;

        public WolfHd2dMapFactory(MapId mapId):base(mapId)
        {
            this.mapId = mapId;

            // 【暫定】パスをここで直接定義しない
            string shaderPath = "Shaders/Hd2dSprite";
            shader = Resources.Load<Shader>(shaderPath);
            //shader = null;
            if (shader == null)
            {
                shader = Shader.Find("Legacy Shaders/Transparent/Diffuse");
            }

            LoadTileInfo();
        }

        public Hd2dMapData Create()
        {
            // マップファイルからタイル情報を読み出し
            // タイル情報からテクスチャ読み込み
            // テクスチャとマップファイルからマップ生成
            string dirPath = $"{Application.streamingAssetsPath}/Data";
            var systemDataRepository = DI.DependencyInjector.It().SystemDataRepository;
            var dataRef = new Domain.Data.DataRef(new Domain.Data.TableId(0), new Domain.Data.RecordId(mapId.Value), new Domain.Data.FieldId(0));
            string fileName = systemDataRepository.FindString(dataRef).Val;
            Util.Wolf.WolfDataReader reader = new Util.Wolf.WolfDataReader($"{dirPath}/{fileName}");

            int tileSetId = reader.ReadInt(0x22, true, out int tmp);
            MapTile.WolfRepository repository = new MapTile.WolfRepository();
            MapTile.TileData tileData = repository.Find(tileSetId);

            Material mapchipMaterial = new Material(shader);
            int autoTileCount = 16;
            Material[] autochipMaterials = new Material[autoTileCount];
            for (int i = 0; i < autoTileCount; i++)
            {
                autochipMaterials[i] = new Material(shader);
            }

            // 【暫定】ファイルを読み込めなかった場合のエラー処理
            string imagePath = $"{Application.streamingAssetsPath}/Data/" + tileData.BaseTileFilePath;
            byte[] baseTexBytes = Util.Common.FileLoader.LoadSync(imagePath);
            Texture2D mapchipTexture = new Texture2D(0, 0);
            mapchipTexture.LoadImage(baseTexBytes);
            mapchipTexture.Apply();
            mapchipMaterial.mainTexture = mapchipTexture;
            mapchipMaterial.mainTexture.filterMode = FilterMode.Point;

            for (int i = 1; i < autoTileCount; i++)
            {
                string autochipImagePath = $"{Application.streamingAssetsPath}/Data/" + tileData.AutoTileFilePaths[i - 1];
                byte[] autoTexBytes = Util.Common.FileLoader.LoadSync(autochipImagePath);
                Texture2D autochipTexture = new Texture2D(0, 0);
                autochipTexture.LoadImage(autoTexBytes);
                autochipTexture.Apply();
                autochipMaterials[i].mainTexture = autochipTexture;
                autochipMaterials[i].mainTexture.filterMode = FilterMode.Point;
            }

            int width = reader.ReadInt(0x26, true, out tmp);
            int height = reader.ReadInt(0x2A, true, out tmp);
            int[,] mapData1 = ReadLayer(reader, width, height, 0x32 + width * height * 4 * 0);
            int[,] mapData2 = ReadLayer(reader, width, height, 0x32 + width * height * 4 * 1);
            int[,] mapData3 = ReadLayer(reader, width, height, 0x32 + width * height * 4 * 2);

            Hd2dMapData mapDataX1 = ReadMap(mapData1, mapchipMaterial, autochipMaterials, tileData,0);
            Hd2dMapData mapDataX2 = ReadMap(mapData2, mapchipMaterial, autochipMaterials, tileData,1);
            Hd2dMapData mapDataX3 = ReadMap(mapData3, mapchipMaterial, autochipMaterials, tileData,2);

            // 変更範囲が巨大になるので、一旦イベントは無視
            MapEvent.EventData[] events = ReadMapEvents(reader, mapchipTexture, 0x32 + width * height * 4 * 3);

            return CombineMapData(events, mapDataX1, mapDataX2, mapDataX3);
        }

        private Hd2dMapData ReadMap(int[,] mapData, Material mapchipMaterial,
            Material[] autochipMaterials, MapTile.TileData tileData,
            int layerNo)
        {
            // 【暫定】読み取る情報はイベントデータを含まずテクスチャとタイル情報のみなのでMapDataではなく別のモデルを返すようにする
            //          各マスの番号をグリッドで返すだけでも良さそう

            int width = mapData.GetLength(1);
            int height = mapData.GetLength(0);
            MovableInfo[,] movableGrid = new MovableInfo[height, width];
            Hd2dBlock[,] blocks = new Hd2dBlock[height, width];

            // オフセット計算のため下側から順に描画
            int firstRow = height - 1;
            for (int i = firstRow; i >= 0; i--)
            {
                for (int j = 0; j < width; j++)
                {
                    Hd2dBlock block = null;
                    Hd2dTileInfo tileInfo = null;

                    // オートチップ判定
                    if (mapData[i, j] >= 100000)
                    {
                        int id = mapData[i, j] / 100000;
                        id--;
                        MapTile.UnitTile tile = tileData.UnitTileConfigs[id];
                        movableGrid[i, j] = GetTileInfoFrom(tile);
                        tileInfo = tileInfoList[id];

                        // ID 0はテクスチャ情報無し
                        if (id == 0)
                        {
                            continue;
                        }

                        // 【暫定】オートチップの横幅を16以外に対応する
                        int chipLength = 16;
                        int xUnitCount = autochipMaterials[id].mainTexture.width / chipLength;
                        int yUnitCount = autochipMaterials[id].mainTexture.height / chipLength;
                        Vector2Int offset = new Vector2Int(mapData[i, j], 0);
                        Hd2dMeshFactory meshFactory = new Hd2dAutoChipMeshFactory(xUnitCount, yUnitCount);
                        block = GenerateMapObject(tileInfo.type, offset, autochipMaterials[id], meshFactory,layerNo);
                    }
                    else
                    {
                        MapTile.UnitTile tile = tileData.UnitTileConfigs[mapData[i, j] + 16];
                        movableGrid[i, j] = GetTileInfoFrom(tile);
                        tileInfo = tileInfoList[mapData[i, j] + 16];

                        // 【暫定】マップチップの横ユニット数を8以外に対応する
                        int xUnitCount = 8;
                        Vector2Int offset = new Vector2Int(mapData[i, j] % xUnitCount, mapData[i, j] / xUnitCount);
                        int chipLength = mapchipMaterial.mainTexture.width / xUnitCount;
                        int yUnitCount = mapchipMaterial.mainTexture.height / chipLength;
                        Hd2dMeshFactory meshFactory = new Hd2dBaseChipMeshFactory(xUnitCount, yUnitCount);
                        block = GenerateMapObject(tileInfo.type, offset, mapchipMaterial, meshFactory,layerNo);
                    }

                    if (i == firstRow)
                    {
                        block.transform.localPosition = new Vector3(j, 0, height - i - 1);
                    }
                    else
                    {
                        // 【暫定】オフセット計算を全方向に対応させる
                        
                        Hd2dBlock frontBlock = blocks[i + 1, j];
                        if (frontBlock == null)
                        {
                            block.transform.localPosition = new Vector3(j, 0, height - i - 1);
                        }
                        else
                        {
                            Vector3 frontPos = frontBlock.transform.localPosition;
                            int frontTile = mapData[i + 1, j];
                            var frontTileInfo = frontTile >= 100000 ? tileInfoList[frontTile / 100000] : tileInfoList[frontTile + 16];
                            int currentTile = mapData[i, j];
                            var currentTileInfo = currentTile >= 100000 ? tileInfoList[currentTile / 100000] : tileInfoList[currentTile + 16];

                            // 制約照合
                            var frontConstraint = frontTileInfo.neighborConstraints[Direction.Up];
                            var currentConstraint = currentTileInfo.neighborConstraints[Direction.Down];
                            if (frontConstraint.hasConstraint && currentConstraint.hasConstraint)
                            {

                            }
                            else if (frontConstraint.hasConstraint)
                            {

                            }
                            else if (currentConstraint.hasConstraint)
                            {

                            }
                            else
                            {
                                block.transform.localPosition = new Vector3(j, 0, height - i - 1);
                            }

                            block.transform.localPosition = frontPos + Vector3.forward;

                        }

                        /*
                        Hd2dBlock frontBlock = blocks[i + 1, j];
                        if (frontBlock == null)
                        {
                            block.transform.localPosition = new Vector3(j, 0, height - i - 1);
                        }
                        else
                        {
                            Vector3 frontPos = frontBlock.transform.localPosition;
                            block.transform.localPosition = frontPos + Vector3.forward;
                        }
                        */
                    }
                    block.transform.localPosition += tileInfo.offset;
                    blocks[i, j] = block;
                }
            }

            Hd2dBlock[] blockArray = blocks.Cast<Hd2dBlock>().Where(block => block != null).ToArray();
            return new Hd2dMapData(mapId, blockArray, width, height, movableGrid, null);
        }

        private Hd2dMapData CombineMapData(MapEvent.EventData[] eventDataArray, params Hd2dMapData[] mapDataArray)
        {
            if (mapDataArray.Length == 0)
            {
                return null;
            }

            int width = mapDataArray[0].Width;
            int height = mapDataArray[0].Height;
            List<Hd2dBlock> blocks = new List<Hd2dBlock>();
            for (int i = 0; i < mapDataArray.Length; i++)
            {
                // 【暫定】描画順序をRenderQueueなどで調整するよう修正
                foreach(var block in mapDataArray[i].Blocks)
                {
                    block.transform.position += Vector3.up * 0.0001f * i;
                }
                blocks.AddRange(mapDataArray[i].Blocks);
            }

            MovableInfo[,] movableGrid = new MovableInfo[mapDataArray[0].Height, mapDataArray[0].Width];
            for (int i = 0; i < mapDataArray[0].Height; i++)
            {
                for (int j = 0; j < mapDataArray[0].Width; j++)
                {
                    bool movable = true;
                    for (int k = 0; k < mapDataArray.Length; k++)
                    {
                        movable &= mapDataArray[k].MovableGrid[i, j].IsMovable;
                    }
                    movableGrid[i, j] = new MovableInfo(movable);
                }
            }

            Hd2dMapData data = new Hd2dMapData(mapId, blocks.ToArray(),
            mapDataArray[0].Width, mapDataArray[0].Height,
             movableGrid, eventDataArray);

            for(int i = 0; i < mapDataArray.Length; i++)
            {
                if (Application.isPlaying)
                {
                    Object.Destroy(mapDataArray[i].BaseObject);
                }
                else
                {
                    Object.DestroyImmediate(mapDataArray[i].BaseObject);
                }
            }
            return data;
        }

        private int[,] ReadLayer(Util.Wolf.WolfDataReader reader, int width, int height, int offset)
        {
            int[,] mapData = new int[height, width];
            for (int j = 0; j < width; j++)
            {
                for (int i = 0; i < height; i++)
                {
                    int val = reader.ReadInt(offset, true, out offset);
                    mapData[i, j] = val;
                }
            }

            return mapData;
        }

        private MovableInfo GetTileInfoFrom(MapTile.UnitTile unitTile)
        {
            bool isMovable = unitTile.MovableTypeValue != MapTile.MovableType.Immovable;

            return new MovableInfo(isMovable);
        }

        private Hd2dBlock GenerateMapObject(MapBlockType blockType, Vector2Int offset,
            Material material, Hd2dMeshFactory meshFactory,int layerNo)
        {
            GameObject ob = new GameObject("Block");
            Hd2dBlock block = null;
            Vector2Int[] offsets = null;

            switch (blockType)
            {
                case MapBlockType.Cube:
                    {
                        int meshCount = 6;
                        offsets = new Vector2Int[meshCount];
                        for (int i = 0; i < meshCount; i++)
                        {
                            offsets[i] = offset;
                        }
                        block = ob.AddComponent<Hd2dCube>();
                    }
                    break;
                case MapBlockType.Slope:
                    {
                        int meshCount = 5;
                        offsets = new Vector2Int[meshCount];
                        for (int i = 0; i < meshCount; i++)
                        {
                            offsets[i] = offset;
                        }
                        block = ob.AddComponent<Hd2dSlope>();
                    }
                    break;
                case MapBlockType.Plane:
                    {
                        int meshCount = 1;
                        offsets = new Vector2Int[meshCount];
                        offsets[0] = offset;
                        block = ob.AddComponent<Hd2dPlane>();
                    }
                    break;
                default:
                    break;
            }

            // 【暫定】正式な座標を割り当てる
            block?.Initialize(material, offsets, Vector3Int.one, meshFactory,layerNo);
            return block;
        }

        private void LoadTileInfo()
        {
            try
            {
                // 【暫定】キーをここで直接定義しない
                string tileFileKey = "Hd2dTileSetting";
                string json = PlayerPrefs.GetString(tileFileKey);
                string infoPath = $"{Application.streamingAssetsPath}/UnityData/tileInfoList.txt";
                json = System.Text.Encoding.Unicode.GetString(Util.Common.FileLoader.LoadSync(infoPath));
                tileInfoList = JsonUtility.FromJson<Hd2dTileInfoList>(json);
                Debug.Log("Loaded tile data");
            }
            catch
            {
                // 【暫定】チップ数を固定しないよう、タイル情報と紐づける
                int chipCount = 2500;
                tileInfoList = new Hd2dTileInfoList(chipCount);
                for (int i = 0; i < chipCount; i++)
                {
                    var constraints = new Dictionary<Direction, Expression.Map.Hd2d.NeighborConstraint>();
                    constraints.Add(Direction.Up, new Expression.Map.Hd2d.NeighborConstraint(false, Vector3Int.zero));
                    constraints.Add(Direction.Right, new Expression.Map.Hd2d.NeighborConstraint(false, Vector3Int.zero));
                    constraints.Add(Direction.Down, new Expression.Map.Hd2d.NeighborConstraint(false, Vector3Int.zero));
                    constraints.Add(Direction.Left, new Expression.Map.Hd2d.NeighborConstraint(false, Vector3Int.zero));
                    tileInfoList[i] = new Hd2dTileInfo(Vector3.zero, MapBlockType.Cube,new Hd2d.NeighborConstraintDict(constraints));
                }
                Debug.Log("Initialized tile info list");
            }
        }
    }

}