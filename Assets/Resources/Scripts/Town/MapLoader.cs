using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MapLoader : MonoBehaviour {

    public TextAsset mp_layout;//マップ情報を記述したテキスト
    Texture2D MapImage;
    const int MAP_WIDTH = 9;
    const int MAP_HEIGHT = 16;
    public int[,] mapdata;
    public string[] mapdataDebug;
    public Sprite mapchips;
    Sprite map;
    const int MASU = 16;

    // Use this for initialization
    void Awake()
    {
        ReadMap();
    }

    void Start()
    {
        mapdataDebug = new string[mapdata.GetLength(1)];
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < mapdataDebug.GetLength(0); i++)
        {
            string sub = "";
            for (int j = 0; j < mapdata.GetLength(0); j++)
            {
                sub += mapdata[j, i].ToString() + ",";
            }
            mapdataDebug[i] = sub;
        }
    }

    /// <summary>
    /// マップ読み込みメソッド
    /// 戻り値はマップサイズ
    /// </summary>
    /// <returns></returns>
    public Vector2 ReadMap()
    {
        var reader = new System.IO.StringReader(mp_layout.text);
        var mapDataList = new List<int[]>();

        while (reader.Peek() > -1)
        {
            string[] eachInfo = reader.ReadLine().Split(',');
            int[] row = new int[eachInfo.Length];

            for (int j = 0; j < eachInfo.Length; j++)//横方向の分割
            {
                if (eachInfo[j] != "")
                {
                    row[j] = int.Parse(eachInfo[j]);
                }
            }

            mapDataList.Add(row);
        }
        mapdata = new int[mapDataList.Count, mapDataList[0].Length];
        for(int i = 0; i < mapDataList.Count; i++)
        {
            for(int j = 0; j < mapDataList[0].Length; j++)
            {
                mapdata[i, j] = mapDataList[i][j];
            }
        }

        MapImage = new Texture2D(MASU * MAP_WIDTH, MASU * MAP_HEIGHT, TextureFormat.RGBA32, false);//マップ初期化
        for (int i = 0; i < MAP_HEIGHT; i++)
        {
            for (int j = 0; j < MAP_WIDTH; j++)
            {
                Debug.Log(mapdata[i, j]);
                Color[] c = mapchips.texture.GetPixels(MASU * (mapdata[i, j] % 8),
                    mapchips.texture.height - MASU * (1 + mapdata[i, j] / 8), MASU, MASU);
                MapImage.SetPixels(MASU * j, MapImage.height - MASU * (i + 1), MASU, MASU, c);
            }
        }
        MapImage.Apply();
        map = Sprite.Create(MapImage, new Rect(0, 0, MAP_WIDTH * MASU, MAP_HEIGHT * MASU), new Vector2(0.5f, 0.5f), MASU);
        map.texture.filterMode = FilterMode.Point;
        GetComponent<SpriteRenderer>().sprite = map;
        return new Vector2(mapdata.GetLength(0), mapdata.GetLength(1));
    }
}
