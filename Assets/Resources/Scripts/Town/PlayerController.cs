using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    Map map;
    [SerializeField]
    string spriteName;
    int direction;
    Dictionary<Vector2Int, int> directionDic;//主人公向き
    int validChipNo;
    List<Vector2> nodePos;
    float corPosY;//y座標位置の補正
    int inter = 2;
    int count = 0;
    Sprite[] sprites;//すべてのスプライト
    int spritePat = 3;//スプライトのアニメーションパターン
    int spriteAniCor = 1;//移動中アニメの変化パターン
    [SerializeField]
    GameObject selectPos;
    GameObject eventObject;

    Expression.Map.MovableInfo[,] movableGrid;
    int[,] mapCostData;

    // Use this for initialization
    void Awake()
    {
        PlayerData.Instance.LoadSelfVars(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }

    void Start()
    {
        transform.position = PlayerData.Instance.pos;
        movableGrid = map.GetMovableInfo();
        mapCostData = new int[movableGrid.GetLength(0), movableGrid.GetLength(1)];
        for (int i = 0; i < mapCostData.GetLength(0); i++)
        {
            for (int j = 0; j < mapCostData.GetLength(1); j++)
            {
                mapCostData[i, j] = -1;
            }
        }

        directionDic = new Dictionary<Vector2Int, int>();
        directionDic.Add(Vector2Int.up, 3);
        directionDic.Add(Vector2Int.right, 2);
        directionDic.Add(Vector2Int.down, 0);
        directionDic.Add(Vector2Int.left, 1);
        direction = directionDic[Vector2Int.down];
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
                Vector2 dest = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Debug.Log(dest);
                dest = GetNormalizedUnityPos(dest);
                Collider2D c = Physics2D.OverlapPoint(dest);
                Vector2Int destGeneral = GetGeneralPos(dest);
                if (!movableGrid[destGeneral.y, destGeneral.x].IsMovable)//移動座標が通行不可だったら処理無効
                {
                    return;
                }
                if (nodePos.Count > 0)//移動が残っていたら完了させる
                {
                    transform.position = nodePos[0];
                }
                selectPos.SetActive(true);
                selectPos.transform.position = dest;
                Vector2Int curGeneral = GetGeneralPos(transform.position);//現在位置
                Debug.Log(dest);
                Debug.Log(destGeneral);
                if (destGeneral == curGeneral)//メニュー呼び出し
                {
                    GetComponent<EventObject>().ReadScript();
                    EventCommands.isProcessing = true;
                    return;
                }
                nodePos = new List<Vector2>();
                SearchRoute(destGeneral, curGeneral, 0);
                if (c != null && !c.GetComponent<EventObject>().CanThrough && c.GetComponent<EventObject>().enabled)
                {
                    eventObject = c.gameObject;
                    mapCostData[destGeneral.y, destGeneral.x] = 99;
                }
                else
                {
                    eventObject = null;
                }
                GetRoute(destGeneral, mapCostData[destGeneral.y, destGeneral.x]);
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
                if (count == 0)
                {
                    Vector2Int d = Vector2Int.zero;
                    // 【暫定】なぜか今いる座標が登録されることがあるので除外する
                    while (nodePos.Count > 0 && d == Vector2Int.zero)
                    {
                        d = new Vector2Int((int)(nodePos[0].x - transform.position.x), (int)(nodePos[0].y - transform.position.y));
                        nodePos.RemoveAt(0);
                    }
                    direction = directionDic[d];
                    spriteAniCor *= -1;
                    GetComponent<SpriteRenderer>().sprite = sprites[spritePat * direction + 1 + spriteAniCor];
                    count++;
                }
                else if (count >= inter)
                {
                    count = 0;
                    transform.position = nodePos[0];
                    GetComponent<SpriteRenderer>().sprite = sprites[spritePat * direction + 1];
                    if (nodePos.Count == 0)
                    {
                        selectPos.SetActive(false);
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
                Vector2Int d
                    = new Vector2Int((int)(eventObject.transform.position.x - transform.position.x),
                    (int)(eventObject.transform.position.y - transform.position.y));
                try
                {
                    direction = directionDic[d];
                }
                catch
                {
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
                if (c != null && c.GetComponent<EventObject>().enabled)
                {
                    EventCommands.isProcessing = true;
                    eventObject = c.gameObject;
                    eventObject.GetComponent<EventObject>().ReadScript();
                    eventObject = null;
                }
            }
        }
    }

    /// <summary>
    /// 目的地までの最短距離を求める再帰関数
    /// </summary>
    /// <param name="destination"></param>
    void SearchRoute(Vector2Int destPos, Vector2Int checkPos, int cost)
    {
        if (cost >= mapCostData[checkPos.y, checkPos.x] && mapCostData[checkPos.y, checkPos.x] != -1)
        {
            return;
        }
        else
        {
            mapCostData[checkPos.y, checkPos.x] = cost;//より小さいコストに
            if (checkPos == destPos)
            {
                return;
            }
            cost++;
            if (checkPos.x - 1 >= 0 && movableGrid[checkPos.y, checkPos.x - 1].IsMovable)
            {
                SearchRoute(destPos, checkPos + Vector2Int.left, cost);
            }
            if (checkPos.y + 1 <= movableGrid.GetLength(0) - 1
                && movableGrid[checkPos.y + 1, checkPos.x].IsMovable)
            {
                SearchRoute(destPos, checkPos + Vector2Int.up, cost);
            }
            if (checkPos.x + 1 <= movableGrid.GetLength(1) - 1
                && movableGrid[checkPos.y, checkPos.x + 1].IsMovable)
            {
                SearchRoute(destPos, checkPos + Vector2Int.right, cost);
            }
            if (checkPos.y - 1 >= 0 && movableGrid[checkPos.y - 1, checkPos.x].IsMovable)
            {
                SearchRoute(destPos, checkPos + Vector2Int.down, cost);
            }
        }
    }

    void GetRoute(Vector2Int pos, int cost)
    {
        cost = mapCostData[pos.y, pos.x];
        if (cost <= 0)
        {
            return;
        }
        if (cost != 99)
        {
            nodePos.Add(GetUnityPos(pos));
        }
        if (pos.x - 1 >= 0 && movableGrid[pos.y, pos.x - 1].IsMovable
            && mapCostData[pos.y, pos.x - 1] != -1 && mapCostData[pos.y, pos.x - 1] <= cost)
        {
            GetRoute(pos + Vector2Int.left, cost);
        }
        else
        if (pos.y + 1 <= movableGrid.GetLength(0) - 1
            && movableGrid[pos.y + 1, pos.x].IsMovable
            && mapCostData[pos.y + 1, pos.x] != -1 && mapCostData[pos.y + 1, pos.x] <= cost)
        {
            GetRoute(pos + Vector2Int.up, cost);
        }
        else
        if (pos.x + 1 <= movableGrid.GetLength(1) - 1
            && movableGrid[pos.y, pos.x + 1].IsMovable &&
            mapCostData[pos.y, pos.x + 1] != -1 && mapCostData[pos.y, pos.x + 1] <= cost)
        {
            GetRoute(pos + Vector2Int.right, cost);
        }
        else
        if (pos.y - 1 >= 0 && movableGrid[pos.y - 1, pos.x].IsMovable
            && mapCostData[pos.y - 1, pos.x] != -1 && mapCostData[pos.y - 1, pos.x] <= cost)
        {
            GetRoute(pos + Vector2Int.down, cost);
        }
    }

    Vector2 GetNormalizedUnityPos(Vector2 unityPos)
    {
        float nextX = movableGrid.GetLength(1) % 2 == 0 ? Mathf.Floor(unityPos.x) + 0.5f : Mathf.Round(unityPos.x);
        float nextY = movableGrid.GetLength(0) % 2 == 0 ? Mathf.Floor(unityPos.y) + 0.5f : Mathf.Round(unityPos.y);

        return new Vector2(nextX, nextY);
    }

    Vector2 GetUnityPos(Vector2Int generalPos)
    {
        float nextX = generalPos.x - movableGrid.GetLength(1) / 2;
        if (movableGrid.GetLength(1) % 2 == 0)
        {
            nextX += 0.5f;
        }

        float nextY = movableGrid.GetLength(0) / 2 - generalPos.y;
        if (movableGrid.GetLength(0) % 2 == 0)
        {
            nextY -= 0.5f;
        }

        return new Vector2(nextX, nextY);
    }

    Vector2Int GetGeneralPos(Vector2 unityPos)
    {
        int nextX = Mathf.CeilToInt(unityPos.x) + movableGrid.GetLength(1) / 2;
        int nextY = movableGrid.GetLength(0) / 2 - Mathf.CeilToInt(unityPos.y);

        return new Vector2Int(nextX, nextY);
    }
}
