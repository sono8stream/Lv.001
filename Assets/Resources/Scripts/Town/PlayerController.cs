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
    Dictionary<Vector2, int> directionDic;//主人公向き
    int validChipNo;
    List<Vector2> nodePos;
    float corPosY;//y座標位置の補正
    int inter = 3;
    int count = 0;
    Sprite[] sprites;//すべてのスプライト
    int spritePat = 3;//スプライトのアニメーションパターン
    int spriteAniCor = 1;//移動中アニメの変化パターン
    [SerializeField]
    GameObject selectPos;
    GameObject eventObject;

    // Use this for initialization
    void Awake()
    {
        PlayerData.Instance.LoadSelfVars(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }

    void Start()
    {
        transform.position = PlayerData.Instance.pos;
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
        directionDic = new Dictionary<Vector2, int>();
        directionDic.Add(Vector2.up, 3);
        directionDic.Add(Vector2.right, 2);
        directionDic.Add(Vector2.down, 0);
        directionDic.Add(Vector2.left, 1);
        direction = directionDic[Vector2.down];
        validChipNo = 7;
        nodePos = new List<Vector2>();
        corPosY = 0.5f;
        sprites = Resources.LoadAll<Sprite>("Sprites/" + spriteName);
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
                selectPos.SetActive(true);
                selectPos.transform.position = dest - corPosMap + Vector2.up * 0.5f;
                Vector2 curPos = new Vector2((int)transform.position.x, (int)(transform.position.y - corPosY)) + corPosMap;//現在位置
                Debug.Log(dest);
                Debug.Log(curPos);
                if (dest == curPos)//メニュー呼び出し
                {
                    GetComponent<EventObject>().ReadScript();
                    EventCommands.isProcessing = true;
                    return;
                }
                nodePos = new List<Vector2>();
                SearchRoute(dest, curPos, 0);
                if (c != null && !c.GetComponent<EventObject>().CanThrough&&c.GetComponent<EventObject>().enabled)
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
                    if (nodePos.Count == 0)
                    {
                        selectPos.SetActive(false);
                    }
                    else
                    {
                        Vector2 d
                            = new Vector2((int)(nodePos[0].x - transform.position.x), (int)(nodePos[0].y - transform.position.y));
                        direction = directionDic[d];
                        spriteAniCor *= -1;
                        GetComponent<SpriteRenderer>().sprite = sprites[spritePat * direction + 1 + spriteAniCor];
                    }
                }
                else /*if (count < inter)*/
                {
                    transform.position = (nodePos[0] - (Vector2)transform.position) / (inter - count)
                        + (Vector2)transform.position;
                    count++;
                }
            }
            else if (eventObject != null)//イベント実行
            {
                Vector2 d
                    = new Vector2((int)(eventObject.transform.position.x - transform.position.x),
                    (int)(eventObject.transform.position.y - transform.position.y));
                try
                {
                    direction = directionDic[d];
                }
                catch
                {
                    Debug.Log("line142");
                    Debug.Log(eventObject.transform.position);
                    Debug.Log(transform.position);
                    Debug.Log(d);
                }
                spriteAniCor *= -1;
                GetComponent<SpriteRenderer>().sprite = sprites[spritePat * direction + 1];
                eventObject.GetComponent<EventObject>().ReadScript();
                EventCommands.isProcessing = true;
                eventObject = null;
                selectPos.SetActive(false);
            }
            else//接触中のイベントを実行
            {
                Collider2D c = Physics2D.OverlapPoint(transform.position);
                if(c!=null)
                {
                    EventCommands.isProcessing = true;
                    eventObject = c.gameObject;
                    eventObject.GetComponent
                        <EventObject>().ReadScript();
                }
            }
        }
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
        if (cost <= 0)
        {
            return;
        }
        if (cost != 99)
        {
            nodePos.Add(pos - new Vector2((mapData.GetLength(0) - 1) / 2, (mapData.GetLength(1) - 1) / 2 + corPosY));
        }
        if (pos.y - 1 >= 0 && mapData[(int)pos.x, (int)pos.y - 1] == validChipNo
            && mapCostData[(int)pos.x, (int)pos.y - 1] != -1 && mapCostData[(int)pos.x, (int)pos.y - 1] <= cost)
        {
            GetRoute(pos + Vector2.down, cost);
        }
        else
        if (pos.x + 1 <= mapData.GetLength(0) - 1
            && mapData[(int)pos.x + 1, (int)pos.y] == validChipNo &&
            mapCostData[(int)pos.x + 1, (int)pos.y] != -1 && mapCostData[(int)pos.x + 1, (int)pos.y] <= cost)
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
            && mapCostData[(int)pos.x - 1, (int)pos.y] != -1 && mapCostData[(int)pos.x - 1, (int)pos.y] <= cost)
        {
            GetRoute(pos + Vector2.left, cost);
        }
    }
}
