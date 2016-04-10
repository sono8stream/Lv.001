using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{

    [SerializeField]
    MapLoader layer;
    [SerializeField]
    string spriteName;
    int[,] mapData;//レイヤー2のマップデータ、7以外では移動不可
    int[,] mapCostData;
    int direction;
    Dictionary<Vector2,int> directionDic;//主人公向き
    int validChipNo;
    List<Vector2> nodePos;
    float corPosY;//y座標位置の補正
    int inter = 3;
    int count = 0;
    Sprite[] sprites;//すべてのスプライト
    int spritePat = 3;//スプライトのアニメーションパターン
    int spriteAniCor = 1;//移動中アニメの変化パターン
    GameObject eventObject;

    // Use this for initialization
    void Start()
    {
        mapData = new int[layer.mapdata.GetLength(0), layer.mapdata.GetLength(1)];
        mapCostData = new int[mapData.GetLength(0), mapData.GetLength(1)];
        for (int i = 0; i < mapCostData.GetLength(0); i++)
        {
            for (int j = 0; j < mapCostData.GetLength(1); j++)
            {
                mapData[i, j] = layer.mapdata[i, layer.mapdata.GetLength(1) - 1 - j];
                mapCostData[i, j] = -1;
            }
        }
        directionDic = new Dictionary<Vector2,int>();
        directionDic.Add(Vector2.up,3);
        directionDic.Add(Vector2.right,2);
        directionDic.Add(Vector2.down,0);
        directionDic.Add(Vector2.left,1);
        direction = directionDic[Vector2.down];
        validChipNo = 7;
        nodePos = new List<Vector2>();
        corPosY = 0.5f;
        sprites = Resources.LoadAll<Sprite>("Sprites/" + spriteName);
        Debug.Log(sprites.GetLength(0));
        eventObject = null;
    }

    // Update is called once per frame
    void Update()
    {
        if (!EventCommands.isProcessing)
        {
            if (Input.GetMouseButtonDown(0))//クリックされたとき、その座標まで主人公を移動
            {
                count = 0;
                Vector2 corPosMap = new Vector2(Mathf.FloorToInt(mapData.GetLength(0) / 2), Mathf.FloorToInt(mapData.GetLength(1) / 2));
                Vector2 dest = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Collider2D c = Physics2D.OverlapPoint(dest);
                dest = new Vector2(Mathf.RoundToInt(dest.x), Mathf.RoundToInt(dest.y - corPosY)) + corPosMap;
                if (mapData[(int)dest.x, (int)dest.y] != validChipNo)//移動座標が通行不可だったら処理無効
                {
                    return;
                }
                if (nodePos.Count > 0)//移動が残っていたら完了させる
                {
                    transform.position = nodePos[0];
                }
                Vector2 curPos = new Vector2((int)transform.position.x, (int)(transform.position.y - corPosY)) + corPosMap;//現在位置
                Debug.Log(dest);
                Debug.Log(curPos);
                nodePos = new List<Vector2>();
                SearchRoute(dest, curPos, 0);
                if (c != null)
                {
                    eventObject = c.gameObject;
                    mapCostData[(int)dest.x, (int)dest.y] = 99;
                }
                else
                {
                    eventObject = null;
                }
                GetRoute(dest, mapCostData[(int)dest.x, (int)dest.y]);
                nodePos.Reverse();
                for (int i = 0; i < mapCostData.GetLength(0); i++)
                {
                    for (int j = 0; j < mapCostData.GetLength(1); j++)
                    {
                        mapCostData[i, j] = -1;
                    }
                }
            }
            else if (nodePos.Count > 0)//移動情報がある場合、移動
            {
                if (count >= inter)
                {
                    count = 0;
                    transform.position = nodePos[0];
                    nodePos.RemoveAt(0);
                    GetComponent<SpriteRenderer>().sprite = sprites[spritePat * direction + 1];
                }
                else /*if (count < inter)*/
                {
                    if (count == 0)//方向を取得して画像変更
                    {
                        direction = directionDic[nodePos[0] - (Vector2)transform.position];
                        spriteAniCor *= -1;
                        GetComponent<SpriteRenderer>().sprite = sprites[spritePat * direction + 1 + spriteAniCor];
                    }
                    transform.position = (nodePos[0] - (Vector2)transform.position) / (inter - count)
                        + (Vector2)transform.position;
                    count++;
                }
            }
            else if (eventObject != null)
            {
                eventObject.GetComponent<EventObject>().ReadScript();
                eventObject = null;
            }
        }
    }

    /// <summary>
    /// 目的地を検索し、そこへ移動
    /// </summary>
    /// <param name="destination">目的の座標</param>
    void GoDestination(Vector2 destination)
    {

    }

    /// <summary>
    /// 目的地までの最短距離を求める再帰関数
    /// </summary>
    /// <param name="destination"></param>
    void SearchRoute(Vector2 destPos, Vector2 checkPos, int cost)
    {
        if (cost >= mapCostData[(int)checkPos.x, (int)checkPos.y] && mapCostData[(int)checkPos.x, (int)checkPos.y] != -1)
        {
            return;
        }
        else
        {
            mapCostData[(int)checkPos.x, (int)checkPos.y] = cost;//より小さいコストに
            if (checkPos == destPos)
            {
                return;
            }
            cost++;
            if (checkPos.y - 1 >= 0 && mapData[(int)checkPos.x, (int)checkPos.y - 1] == validChipNo)
            {
                SearchRoute(destPos, checkPos + Vector2.down, cost);
            }
            if (checkPos.x + 1 <= mapData.GetLength(0) - 1
                && mapData[(int)checkPos.x + 1, (int)checkPos.y] == validChipNo)
            {
                SearchRoute(destPos, checkPos + Vector2.right, cost);
            }
            if (checkPos.y + 1 <= mapData.GetLength(1) - 1
                && mapData[(int)checkPos.x, (int)checkPos.y + 1] == validChipNo)
            {
                SearchRoute(destPos, checkPos + Vector2.up, cost);
            }
            if (checkPos.x - 1 >= 0 && mapData[(int)checkPos.x - 1, (int)checkPos.y] == validChipNo)
            {
                SearchRoute(destPos, checkPos + Vector2.left, cost);
            }
        }
    }

    void GetRoute(Vector2 pos, int cost)
    {
        cost = mapCostData[(int)pos.x, (int)pos.y];
        Debug.Log(cost);
        if (cost <= 0)
        {
            return;
        }
        if (cost != 99)
        {
            nodePos.Add(pos - new Vector2((mapData.GetLength(0) - 1) / 2, (mapData.GetLength(1) - 1) / 2 + corPosY));
        }
        if (pos.y - 1 >= 0 && mapData[(int)pos.x, (int)pos.y - 1] == validChipNo
            && mapCostData[(int)pos.x, (int)pos.y - 1]!=-1&&mapCostData[(int)pos.x, (int)pos.y - 1] <= cost)
        {
            GetRoute(pos + Vector2.down, cost);
        }
        else
        if (pos.x + 1 <= mapData.GetLength(0) - 1
            && mapData[(int)pos.x + 1, (int)pos.y] == validChipNo &&
            mapCostData[(int)pos.x+1, (int)pos.y] != -1 &&  mapCostData[(int)pos.x + 1, (int)pos.y] <= cost)
        {
            GetRoute(pos + Vector2.right, cost);
        }
        else
        if (pos.y + 1 <= mapData.GetLength(1) - 1
            && mapData[(int)pos.x, (int)pos.y + 1] == validChipNo &&
            mapCostData[(int)pos.x, (int)pos.y + 1] != -1 && mapCostData[(int)pos.x, (int)pos.y + 1] <= cost)
        {
            GetRoute(pos + Vector2.up, cost);
        }
        else
        if (pos.x - 1 >= 0 && mapData[(int)pos.x - 1, (int)pos.y] == validChipNo
            && mapCostData[(int)pos.x-1, (int)pos.y] != -1 && mapCostData[(int)pos.x - 1, (int)pos.y] <= cost)
        {
            GetRoute(pos + Vector2.left, cost);
        }
    }


}
