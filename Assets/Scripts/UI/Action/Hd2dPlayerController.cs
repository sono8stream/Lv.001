using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UI.Map;
using Expression.Map;
using UI.Action;

public class Hd2dPlayerController : MonoBehaviour
{

    [SerializeField]
    Hd2dMap map;
    int direction;
    Dictionary<Vector2Int, int> directionDic;//主人公向き
    List<Vector3> nodePos;
    Vector3 currentNormalizedPos;
    int inter = 3;
    int count = 0;
    int spritePat = 3;//スプライトのアニメーションパターン
    int spriteAniCor = 1;//移動中アニメの変化パターン

    [SerializeField]
    GameObject selectPos;

    [SerializeField]
    EventObject targetEvent;

    [SerializeField]
    Shader shader;
    [SerializeField]
    Texture2D texture;

    [SerializeField]
    VariableJoystick variableJoyStick;

    MovableInfo[,] movableGrid;
    int[,] mapCostData;

    ActionProcessor processor;

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
        transform.position = new Vector3(movableGrid.GetLength(1) / 2, 1, movableGrid.GetLength(0) / 2);
        currentNormalizedPos = transform.position;

        directionDic = new Dictionary<Vector2Int, int>();
        directionDic.Add(Vector2Int.up, 3);
        directionDic.Add(Vector2Int.right, 2);
        directionDic.Add(Vector2Int.down, 0);
        directionDic.Add(Vector2Int.left, 1);
        direction = directionDic[Vector2Int.down];
        nodePos = new List<Vector3>();

        Material mat = new Material(shader);
        mat.mainTexture = texture;
        mat.mainTexture.filterMode = FilterMode.Point;
        GetComponentInChildren<Renderer>().sharedMaterial = mat;
        SetMeshWait();

        targetEvent = null;
        processor = GameObject.Find("ActionProcessor").GetComponent<ActionProcessor>();
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
            // 自動実行イベントを処理する
            ProcessAutoEvents();
            if (ActionProcessor.isProcessing)
            {
                return;
            }

            ChangeDirection();

            Vector2Int curGeneral = new Vector2Int(Mathf.RoundToInt(transform.position.x), movableGrid.GetLength(0) - Mathf.RoundToInt(transform.position.z) - 1);
            ControlWithKey(curGeneral);
            ControlWithTouch(curGeneral);
        }
        else //移動情報があるので移動
        {
            if (count == 0)
            {
                Vector2Int d = new Vector2Int(Mathf.RoundToInt(nodePos[0].x - transform.position.x), Mathf.RoundToInt(nodePos[0].z - transform.position.z));
                direction = directionDic[d];
                spriteAniCor *= -1;
                SetMeshWalk();
                count++;
            }
            else if (count >= inter)
            {
                count = 0;
                transform.position = nodePos[0];
                currentNormalizedPos = nodePos[0];
                SetMeshWait();

                nodePos.RemoveAt(0);
                if (nodePos.Count == 0)
                {
                    selectPos.SetActive(false);
                    SetMeshWait();
                }
            }
            else /*if (count < inter)*/
            {
                transform.position = (nodePos[0] - currentNormalizedPos) / (inter - count)
                    + currentNormalizedPos;
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

    bool CheckIsMovable(Vector2Int pos)
    {
        if(0 <= pos.x && pos.x < movableGrid.GetLength(1)
            && 0 <= pos.y && pos.y < movableGrid.GetLength(0)
            && movableGrid[pos.y, pos.x].IsMovable)
        {
            if (targetEvent == null)
            {
                return true;
            }
            else
            {
                // 進行方向にすり抜けられないイベントがある場合はスキップ
                Vector2Int curGeneral = new Vector2Int(Mathf.RoundToInt(targetEvent.transform.position.x), movableGrid.GetLength(0) - Mathf.RoundToInt(targetEvent.transform.position.z) - 1);
                if (!targetEvent.CanPass()
                    && pos == curGeneral)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }
        else
        {
            // そもそも移動できない場合
            return false;
        }
    }

    void ProcessAutoEvents()
    {
        foreach (EventObject targetEvent in map.MapEvents)
        {
            if (targetEvent.IsExecutable(Expression.Map.MapEvent.EventTriggerType.Auto))
            {
                processor.StartActions(targetEvent);
            }
        }
    }

    void ChangeDirection()
    {
        Vector2Int dire;
        if (Input.GetKey(KeyCode.RightArrow))
        {
            dire = Vector2Int.right;
        }
        else if (Input.GetKey(KeyCode.UpArrow))
        {
            dire = Vector2Int.up;
        }
        else if (Input.GetKey(KeyCode.LeftArrow))
        {
            dire = Vector2Int.left;
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            dire = Vector2Int.down;
        }
        else
        {
            return;
        }

        direction = directionDic[dire];
        SetMeshWait();
    }

    private void SetMeshWait()
    {
        SetMesh(1);
    }

    private void SetMeshWalk()
    {
        SetMesh(1 + spriteAniCor);
    }

    private void SetMesh(int patternValue)
    {
        var meshFactory = new Hd2dCharaChipMeshFactory(3, 4);
        var dir = GetDirectionFromValue(direction);
        GetComponentInChildren<MeshFilter>().sharedMesh = meshFactory.Create(dir, patternValue);

    }

    private void ControlWithKey(Vector2Int curGeneral)
    {
        if (Input.GetKey(KeyCode.RightArrow)
            && CheckIsMovable(curGeneral + Vector2Int.right))
        {
            nodePos.Add(transform.position + Vector3.right);
        }
        else if (Input.GetKey(KeyCode.UpArrow)
            && CheckIsMovable(curGeneral + Vector2Int.down))
        {
            nodePos.Add(transform.position + Vector3.forward);
        }
        else if (Input.GetKey(KeyCode.LeftArrow)
            && CheckIsMovable(curGeneral + Vector2Int.left))
        {
            nodePos.Add(transform.position + Vector3.left);
        }
        else if (Input.GetKey(KeyCode.DownArrow)
            && CheckIsMovable(curGeneral + Vector2Int.up))
        {
            nodePos.Add(transform.position + Vector3.back);
        }
        else if (Input.GetKey(KeyCode.X))
        {
            // 【暫定】メニュー表示処理を実行
            //GetComponent<ActionProcessor>().StartActions();
            return;
        }
        else if (Input.GetKey(KeyCode.Z))
        {
            if (targetEvent != null
                && targetEvent.IsExecutable(Expression.Map.MapEvent.EventTriggerType.OnCheck))//イベント実行
            {
                Vector2Int d
                    = new Vector2Int((int)(targetEvent.transform.position.x - transform.position.x),
                    (int)(targetEvent.transform.position.z - transform.position.z));

                direction = directionDic[d];
                spriteAniCor *= -1;
                SetMeshWait();
                processor.StartActions(targetEvent);
                targetEvent = null;
                selectPos.SetActive(false);
            }
        }
    }

    private void ControlWithTouch(Vector2Int curGeneral)
    {
        float horizonVal = variableJoyStick.Horizontal;
        float verticalVal = variableJoyStick.Vertical;

        // 特に入力無しならスキップ
        if (Mathf.Abs(horizonVal) == 0 && Mathf.Abs(verticalVal) == 0)
        {
            if (Input.GetMouseButtonUp(0) && targetEvent != null)//クリックされたとき、その座標まで主人公を移動
            {
                Vector2Int d
                    = new Vector2Int((int)(targetEvent.transform.position.x - transform.position.x),
                    (int)(targetEvent.transform.position.z - transform.position.z));

                direction = directionDic[d];
                spriteAniCor *= -1;
                SetMeshWait();
                processor.StartActions(targetEvent);
                targetEvent = null;
                selectPos.SetActive(false);
            }
            return;
        }

        // 中間値の場合はスキップ
        float minAbsVal = Mathf.Min(Mathf.Abs(horizonVal), Mathf.Abs(verticalVal));
        float maxAbsVal = Mathf.Max(Mathf.Abs(horizonVal), Mathf.Abs(verticalVal));
        // 30°以上になるならスキップ
        if (minAbsVal / maxAbsVal > Mathf.Tan(30 * Mathf.PI / 180))
        {
            return;
        }

        // 差分が大きい方向を採用
        if (Mathf.Abs(horizonVal) < Mathf.Abs(verticalVal))
        {
            horizonVal = 0;
        }
        else
        {
            verticalVal = 0;
        }

        if (horizonVal > 0
            && CheckIsMovable(curGeneral + Vector2Int.right))
        {
            nodePos.Add(transform.position + Vector3.right);
        }
        else if (verticalVal > 0
            && CheckIsMovable(curGeneral + Vector2Int.down))
        {
            nodePos.Add(transform.position + Vector3.forward);
        }
        else if (horizonVal < 0
            && CheckIsMovable(curGeneral + Vector2Int.left))
        {
            nodePos.Add(transform.position + Vector3.left);
        }
        else if (verticalVal < 0
            && CheckIsMovable(curGeneral + Vector2Int.up))
        {
            nodePos.Add(transform.position + Vector3.back);
        }
        // 【暫定】タッチでもイベント起動できるようにする
    }

    private Direction GetDirectionFromValue(int dir)
    {
        switch (dir)
        {
            case 0:
                return Direction.Down;
            case 1:
                return Direction.Left;
            case 2:
                return Direction.Right;
            case 3:
                return Direction.Up;
            default:
                return Direction.Down;
        }
    }

    private void OnTriggerEnter(Collider col)
    {
        EventObject target = col.GetComponent<EventObject>();
        if (target != null
            && processor.enabled)
        {
            targetEvent = target;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        EventObject target = other.GetComponent<EventObject>();
        if (target == targetEvent)
        {
            targetEvent = null;
        }
    }
}
