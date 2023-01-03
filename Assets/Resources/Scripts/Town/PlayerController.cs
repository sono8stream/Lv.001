﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UI.Map;
using UI.Action;

/// <summary>
/// 【暫定】Hd2dPlayerControllerと重複する処理が共通化しきれておらず、使用できない。廃止予定。
/// </summary>
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
    int inter = 3;
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
        transform.position = new Vector2(movableGrid.GetLength(1) / 2 + 0.5f, movableGrid.GetLength(0) / 2 + 0.5f);

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
        if (ActionProcessor.isProcessing)
        {
            return;
        }

        if (nodePos.Count == 0)
        {
            if (Input.GetMouseButtonDown(0))//クリックされたとき、その座標まで主人公を移動
            {
                count = 0;
                Vector2 rawDest = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Debug.Log($"rawDest:{rawDest}");
                Vector2 dest = Util.Map.PositionConverter.GetNormalizedUnityPos(rawDest);
                Debug.Log($"dest:{dest}");
                Collider2D c = Physics2D.OverlapPoint(dest);
                Vector2Int destGeneral = Util.Map.PositionConverter.GetGeneralPos(dest, movableGrid.GetLength(0));
                Debug.Log($"destGeneral:{destGeneral}");
                if (destGeneral.x < 0 || destGeneral.y < 0 || destGeneral.x >= movableGrid.GetLength(1) || destGeneral.y >= movableGrid.GetLength(0)
                || !movableGrid[destGeneral.y, destGeneral.x].IsMovable)//移動座標が通行不可だったら処理無効
                {
                    return;
                }
                if (nodePos.Count > 0)//移動が残っていたら完了させる
                {
                    transform.position = nodePos[0];
                }
                selectPos.SetActive(true);
                selectPos.transform.position = dest;
                Vector2Int curGeneral = Util.Map.PositionConverter.GetGeneralPos(Util.Map.PositionConverter.GetNormalizedUnityPos(transform.position), movableGrid.GetLength(0));//現在位置
                Debug.Log($"curGeneral:{curGeneral}");
                Debug.Log($"object detected:{c != null}");
                if (destGeneral == curGeneral)//メニュー呼び出し
                {
                    GetComponent<ActionProcessor>().ReadScript();
                    ActionProcessor.isProcessing = true;
                    return;
                }

                nodePos = new List<Vector2>();
                SearchRoute(destGeneral, curGeneral, 0);
                GetRoute(destGeneral, mapCostData[destGeneral.y, destGeneral.x]);
                nodePos.Reverse();
                for (int i = 0; i < nodePos.Count; i++)
                {
                    Debug.Log(nodePos[i]);
                }

                if (c != null && !c.GetComponent<EventObject>().CanThrough
                && c.GetComponent<ActionProcessor>().enabled)
                {
                    // イベント実行できるなら最後の座標を削除
                    eventObject = c.gameObject;
                    nodePos.RemoveAt(nodePos.Count - 1);
                }
                else
                {
                    eventObject = null;
                }

                for (int i = 0; i < mapCostData.GetLength(0); i++)
                {
                    for (int j = 0; j < mapCostData.GetLength(1); j++)
                    {
                        mapCostData[i, j] = -1;
                    }
                }
            }
            else if (Input.GetKey(KeyCode.RightArrow))
            {
                Vector2Int curGeneral = Util.Map.PositionConverter.GetGeneralPos(Util.Map.PositionConverter.GetNormalizedUnityPos(transform.position), movableGrid.GetLength(0));//現在位置
                if (curGeneral.x + 1 < movableGrid.GetLength(1)
                && movableGrid[curGeneral.y, curGeneral.x + 1].IsMovable)
                {
                    nodePos.Add(Util.Map.PositionConverter.GetNormalizedUnityPos(transform.position) + Vector2.right);
                }
            }
            else if (Input.GetKey(KeyCode.UpArrow))
            {
                Vector2Int curGeneral = Util.Map.PositionConverter.GetGeneralPos(Util.Map.PositionConverter.GetNormalizedUnityPos(transform.position), movableGrid.GetLength(0));//現在位置
                if (curGeneral.y - 1 >= 0
                && movableGrid[curGeneral.y - 1, curGeneral.x].IsMovable)
                {
                    nodePos.Add(Util.Map.PositionConverter.GetNormalizedUnityPos(transform.position) + Vector2.up);
                }
            }
            else if (Input.GetKey(KeyCode.LeftArrow))
            {
                Vector2Int curGeneral = Util.Map.PositionConverter.GetGeneralPos(Util.Map.PositionConverter.GetNormalizedUnityPos(transform.position), movableGrid.GetLength(0));//現在位置
                if (curGeneral.x - 1 >= 0
                && movableGrid[curGeneral.y, curGeneral.x - 1].IsMovable)
                {
                    nodePos.Add(Util.Map.PositionConverter.GetNormalizedUnityPos(transform.position) + Vector2.left);
                }
            }
            else if (Input.GetKey(KeyCode.DownArrow))
            {
                Vector2Int curGeneral = Util.Map.PositionConverter.GetGeneralPos(Util.Map.PositionConverter.GetNormalizedUnityPos(transform.position), movableGrid.GetLength(0));//現在位置
                if (curGeneral.y + 1 < movableGrid.GetLength(0)
                && movableGrid[curGeneral.y + 1, curGeneral.x].IsMovable)
                {
                    nodePos.Add(Util.Map.PositionConverter.GetNormalizedUnityPos(transform.position) + Vector2.down);
                }
            }
            else if (Input.GetKey(KeyCode.X))
            {
                //GetComponent<ActionProcessor>().StartActions();
                return;
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
                GetComponentInChildren<SpriteRenderer>().sprite = sprites[spritePat * direction + 1];
                //eventObject.GetComponent<ActionProcessor>().StartActions();
                eventObject = null;
                selectPos.SetActive(false);
            }
            else//接触中のイベントを実行
            {
                Collider2D c = Physics2D.OverlapPoint(transform.position);
                if (c != null && c.GetComponent<ActionProcessor>().enabled)
                {
                    eventObject = c.gameObject;
                    //eventObject.GetComponent<ActionProcessor>().StartActions();
                    eventObject = null;
                }
            }
        }
        else //移動情報があるので移動
        {
            if (count == 0)
            {
                Vector2Int d = new Vector2Int((int)Mathf.Round(nodePos[0].x - transform.position.x), (int)Mathf.Round(nodePos[0].y - transform.position.y));
                direction = directionDic[d];
                spriteAniCor *= -1;
                GetComponentInChildren<SpriteRenderer>().sprite = sprites[spritePat * direction + 1 + spriteAniCor];
                count++;
            }
            else if (count >= inter)
            {
                count = 0;
                transform.position = nodePos[0];
                GetComponentInChildren<SpriteRenderer>().sprite = sprites[spritePat * direction + 1];

                nodePos.RemoveAt(0);
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
            nodePos.Add(Util.Map.PositionConverter.GetUnityPos(pos, movableGrid.GetLength(0)));
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
}
