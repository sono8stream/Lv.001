using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Expression.Map
{
    public class WolfHd2dMapFactory
    {
        // 【暫定】マップチップのピクセル数は16で固定とする
        //          マップ描画時など至る所で使用するので，どう使いまわすかが課題
        //          MapDataに含める?
        private const int PIXEL_PER_GRID = 16;
        private MapId mapId;

        private Hd2dTileInfo[] tileInfoArray;
        private Shader shader;

        public WolfHd2dMapFactory(MapId mapId, Hd2dTileInfo[] tileInfoArray,Shader shader)
        {
            this.mapId = mapId;
            this.tileInfoArray = tileInfoArray;
            this.shader = shader;
        }

        public Hd2dMapData Create(string mapFilePath)
        {
            // マップファイルからタイル情報を読み出し
            // タイル情報からテクスチャ読み込み
            // テクスチャとマップファイルからマップ生成

            Util.Wolf.WolfDataReader reader = new Util.Wolf.WolfDataReader(mapFilePath);

            int tileSetId = reader.ReadInt(0x22, true, out int tmp);
            MapTile.WolfRepository repository = new MapTile.WolfRepository();
            MapTile.TileData tileData = repository.Find(tileSetId);

            if (shader == null)
            {
                shader = Shader.Find("Legacy Shaders/Transparent/Diffuse");
            }
            Material mapchipMaterial = new Material(shader);
            int autoTileCount = 16;
            Material[] autochipMaterials = new Material[autoTileCount];
            for(int i = 0; i < autoTileCount; i++)
            {
                autochipMaterials[i] = new Material(shader);
            }

            {
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
            }

            int width = reader.ReadInt(0x26, true, out tmp);
            int height = reader.ReadInt(0x2A, true, out tmp);
            int[,] mapData1 = ReadLayer(reader, width, height, 0x32 + width * height * 4 * 0);
            int[,] mapData2 = ReadLayer(reader, width, height, 0x32 + width * height * 4 * 1);
            int[,] mapData3 = ReadLayer(reader, width, height, 0x32 + width * height * 4 * 2);

            Hd2dMapData mapDataX1 = ReadMap(mapData1, mapchipMaterial, autochipMaterials, tileData);
            Hd2dMapData mapDataX2 = ReadMap(mapData2, mapchipMaterial, autochipMaterials, tileData);
            Hd2dMapData mapDataX3 = ReadMap(mapData3, mapchipMaterial, autochipMaterials, tileData);

            // 変更範囲が巨大になるので、一旦イベントは無視
            //MapEvent.EventData[] events = ReadMapEvents(reader, mapchipMaterial, 0x32 + width * height * 4 * 3);

            return CombineMapData(null, mapDataX1, mapDataX2, mapDataX3);
        }

        private Hd2dMapData ReadMap(int[,] mapData, Material mapchipMaterial, Material[] autochipMaterials, MapTile.TileData tileData)
        {
            // 【暫定】読み取る情報はイベントデータを含まずテクスチャとタイル情報のみなのでMapDataではなく別のモデルを返すようにする
            //          各マスの番号をグリッドで返すだけでも良さそう

            int width = mapData.GetLength(1);
            int height = mapData.GetLength(0);
            MovableInfo[,] movableGrid = new MovableInfo[height, width];
            Hd2dBlock[,] blocks = new Hd2dBlock[height, width];

            // オフセット計算のため下側から順に描画
            int firstRow = height - 1;
            for (int i =firstRow; i >= 0; i--)
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
                        tileInfo = tileInfoArray[id];

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
                        block = GenerateMapObject(tileInfo.type, offset, autochipMaterials[id], meshFactory);
                    }
                    else
                    {
                        MapTile.UnitTile tile = tileData.UnitTileConfigs[mapData[i, j] + 16];
                        movableGrid[i, j] = GetTileInfoFrom(tile);
                        tileInfo = tileInfoArray[mapData[i, j] + 16];

                        // 【暫定】マップチップの横ユニット数を8以外に対応する
                        int xUnitCount = 8;
                        Vector2Int offset = new Vector2Int(mapData[i, j] % xUnitCount, mapData[i, j] / xUnitCount);
                        int chipLength = mapchipMaterial.mainTexture.width / xUnitCount;
                        int yUnitCount = mapchipMaterial.mainTexture.height / chipLength;
                        Hd2dMeshFactory meshFactory = new Hd2dBaseChipMeshFactory(xUnitCount, yUnitCount);
                        block = GenerateMapObject(tileInfo.type, offset, mapchipMaterial, meshFactory);
                    }

                    if (i == firstRow)
                    {
                        block.transform.localPosition = new Vector3(j, 0, -i);
                    }
                    else
                    {
                        Hd2dBlock frontBlock = blocks[i + 1, j];
                        if (frontBlock == null)
                        {
                            block.transform.localPosition = new Vector3(j, 0, -i);
                        }
                        else
                        {
                            Vector3 frontPos = frontBlock.transform.localPosition;
                            block.transform.localPosition = frontPos + Vector3.forward;
                        }
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
                foreach(Hd2dBlock block in mapDataArray[i].Blocks)
                {
                    block.transform.localPosition += Vector3.up * 0.0001f * i;
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
            Material material, Hd2dMeshFactory meshFactory)
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

            block?.Initialize(material, offsets, Vector3Int.one, meshFactory);
            return block;
        }

        private MapEvent.EventData[] ReadMapEvents(Util.Wolf.WolfDataReader reader, Texture2D mapTexture, int offset)
        {
            List<MapEvent.EventData> list = new List<MapEvent.EventData>();
            int headerByte = reader.ReadByte(offset, out offset);
            while (headerByte != 0x66)
            {
                // ヘッダーの余り部分をスキップ
                for (int i = 0; i < 4; i++)
                {
                    reader.ReadByte(offset, out offset);
                }
                MapEvent.EventId eventId = new MapEvent.EventId(
                    reader.ReadInt(offset, true, out offset));
                Debug.Log($"イベントID：{eventId.Value}");
                string eventName = reader.ReadString(offset, out offset);
                int posX = reader.ReadInt(offset, true, out offset);
                int posY = reader.ReadInt(offset, true, out offset);
                int pageCount = reader.ReadInt(offset, true, out offset);
                Debug.Log($"ページ数：{pageCount}");
                // 00 00 00 00のスキップ
                reader.ReadInt(offset, true, out offset);

                List<MapEvent.EventPageData> eventPages = new List<MapEvent.EventPageData>();
                for (int i = 0; i < pageCount; i++)
                {
                    // イベントページの読み込み
                    eventPages.Add(ReadEventPageData(reader, mapTexture, eventId, offset, out offset));
                }

                // フッタースキップ
                reader.ReadByte(offset, out offset);

                list.Add(new MapEvent.EventData(eventId, posX, posY, eventPages.ToArray()));

                // 次の計算用にヘッダを更新
                int nextHeaderByte = reader.ReadByte(offset, out offset);
                headerByte = nextHeaderByte;
            }
            Debug.Log(list.Count);

            return list.ToArray();
        }

        private MapEvent.EventPageData ReadEventPageData(Util.Wolf.WolfDataReader reader, Texture2D mapTexture,
            MapEvent.EventId eventId, int offset, out int nextOffset)
        {
            // ヘッダースキップ
            int hh = reader.ReadByte(offset, out offset);
            int tileNo = reader.ReadInt(offset, true, out offset);
            string chipImgName = reader.ReadString(offset, out offset);
            Debug.Log(chipImgName);
            int directionVal = reader.ReadByte(offset, out offset);
            int animNo = reader.ReadByte(offset, out offset);
            int charaAlpha = reader.ReadByte(offset, out offset);
            int showType = reader.ReadByte(offset, out offset);// 通常/加算/減算/乗算
            int triggerFlagType = reader.ReadByte(offset, out offset);
            int triggerFlagOpr1 = reader.ReadByte(offset, out offset);
            int triggerFlagOpr2 = reader.ReadByte(offset, out offset);
            int triggerFlagOpr3 = reader.ReadByte(offset, out offset);
            int triggerFlagOpr4 = reader.ReadByte(offset, out offset);
            int triggerFlagLeft1 = reader.ReadInt(offset, true, out offset);
            int triggerFlagLeft2 = reader.ReadInt(offset, true, out offset);
            int triggerFlagLeft3 = reader.ReadInt(offset, true, out offset);
            int triggerFlagLeft4 = reader.ReadInt(offset, true, out offset);
            int triggerFlagRight1 = reader.ReadInt(offset, true, out offset);
            int triggerFlagRight2 = reader.ReadInt(offset, true, out offset);
            int triggerFlagRight3 = reader.ReadInt(offset, true, out offset);
            int triggerFlagRight4 = reader.ReadInt(offset, true, out offset);

            ReadEventMoveRoute(reader, offset, out offset);

            int eventCommandCount = reader.ReadInt(offset, true, out offset);
            Debug.Log($"イベントコマンド数：{eventCommandCount}");
            // デバッグここまでOK
            MapEvent.EventCommandBase[] commands = ReadEventCommands(reader, eventId, eventCommandCount, offset, out offset);

            // イベントコマンドフッタースキップ
            reader.ReadInt(offset, true, out offset);

            int shadowNo = reader.ReadByte(offset, out offset);
            int rangeExtendX = reader.ReadByte(offset, out offset);
            int rangeExtendY = reader.ReadByte(offset, out offset);

            // フッタースキップ
            int ff = reader.ReadByte(offset, out offset);

            nextOffset = offset;
            Direction direction = ConvertDirectionValueToDirection(directionVal);

            Texture2D texture = null;
            bool haveDirection = false;
            if (tileNo == -1)
            {
                if (string.IsNullOrEmpty(chipImgName))
                {
                    haveDirection = false;
                }
                else
                {
                    // キャラチップから画像を取得する
                    // 【暫定】読み込めなかった場合のエラー処理を追加
                    texture = new Texture2D(1, 1);
                    string imagePath = $"{Application.streamingAssetsPath}/Data/" + chipImgName;
                    byte[] charaTexBytes = Util.Common.FileLoader.LoadSync(imagePath);
                    texture.LoadImage(charaTexBytes);
                    texture.Apply();

                    haveDirection = true;
                }
            }
            else
            {
                // マップタイルから画像を取得する
                texture = new Texture2D(PIXEL_PER_GRID, PIXEL_PER_GRID);
                Color[] c = mapTexture.GetPixels(PIXEL_PER_GRID * (tileNo % 8),
                    mapTexture.height - PIXEL_PER_GRID * (tileNo / 8 + 1), PIXEL_PER_GRID, PIXEL_PER_GRID);
                texture.SetPixels(0, 0, PIXEL_PER_GRID, PIXEL_PER_GRID, c);
                texture.Apply();
                haveDirection = false;
            }
            Debug.Log(haveDirection);

            return new MapEvent.EventPageData(texture, direction, haveDirection, commands);
        }

        private Direction ConvertDirectionValueToDirection(int directionVal)
        {
            switch (directionVal)
            {
                case 1:
                    return Direction.DownLeft;
                case 2:
                    return Direction.Down;
                case 3:
                    return Direction.DownRight;
                case 4:
                    return Direction.Left;
                case 5:
                    return Direction.Down;
                case 6:
                    return Direction.Right;
                case 7:
                    return Direction.UpLeft;
                case 8:
                    return Direction.Up;
                case 9:
                    return Direction.UpRight;
                default:
                    return Direction.Down;
            }
        }

        // 【暫定】モデル定義までデータを空読み
        private void ReadEventMoveRoute(Util.Wolf.WolfDataReader reader, int offset, out int nextOffset)
        {
            int animationSpeed = reader.ReadByte(offset, out offset);
            int moveSpeed = reader.ReadByte(offset, out offset);
            int moveFrequency = reader.ReadByte(offset, out offset);
            int moveType = reader.ReadByte(offset, out offset);
            int optionType = reader.ReadByte(offset, out offset);
            int moveFlag = reader.ReadByte(offset, out offset);
            int commandCount = reader.ReadInt(offset, true, out offset);
            Debug.Log($"移動コマンド数：{commandCount}");

            // 動作コマンド
            for (int i = 0; i < commandCount; i++)
            {
                int commandType = reader.ReadByte(offset, out offset);
                int variableCount = reader.ReadByte(offset, out offset);
                Debug.Log($"コマンドタイプ：{commandType}、変数の数： {variableCount}");
                for (int j = 0; j < variableCount; j++)
                {
                    int variableValue = reader.ReadInt(offset, true, out offset);
                    Debug.Log($"変数{j}：{variableValue}");
                }

                // 終端
                int footer1 = reader.ReadByte(offset, out offset);
                int footer2 = reader.ReadByte(offset, out offset);
                Debug.Log($"移動コマンド　フッタ：{footer1} {footer2}");
            }

            nextOffset = offset;
        }

        // 【暫定】詳細定義していないコマンドは空読み
        private MapEvent.EventCommandBase[] ReadEventCommands(Util.Wolf.WolfDataReader reader,
            MapEvent.EventId eventId, int eventCommandCount, int offset, out int nextOffset)
        {
            int currentOffset = offset;
            MapEvent.WolfEventCommandFactory factory = new MapEvent.WolfEventCommandFactory(reader, mapId, eventId, currentOffset);
            List<MapEvent.EventCommandBase> commands = new List<MapEvent.EventCommandBase>();
            for (int i = 0; i < eventCommandCount; i++)
            {
                // 一つ一つのコマンドを読み取る
                commands.Add(factory.Create(out currentOffset));
            }
            nextOffset = currentOffset;

            return commands.ToArray();
        }
    }

}