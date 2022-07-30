using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UI.Map;

public class Hd2dPlayerController : MonoBehaviour
{

    [SerializeField]
    Hd2dMap map;
    int direction;
    Dictionary<Vector2Int, int> directionDic;//主人公向き
    List<Vector3> nodePos;
    int inter = 3;
    int count = 0;
    int spritePat = 3;//スプライトのアニメーションパターン
    int spriteAniCor = 1;//移動中アニメの変化パターン
    [SerializeField]
    GameObject selectPos;
    GameObject eventObject;

    [SerializeField]
    Shader shader;
    [SerializeField]
    Texture2D texture;

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
        transform.position = new Vector3(movableGrid.GetLength(1) / 2, 1, movableGrid.GetLength(0) / 2);

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
            // 【暫定】タップで移動を有効にする
            Vector2Int curGeneral = new Vector2Int(Mathf.RoundToInt(transform.position.x), movableGrid.GetLength(0) - Mathf.RoundToInt(transform.position.z) - 1);
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
                GetComponent<ActionProcessor>().StartActions();
                return;
            }
            else if (Input.GetKey(KeyCode.Z))
            {
                if (eventObject != null)//イベント実行
                {
                    Vector2Int d
                        = new Vector2Int((int)(eventObject.transform.position.x - transform.position.x),
                        (int)(eventObject.transform.position.z - transform.position.z));
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
                    SetMeshWait();
                    eventObject.GetComponent<ActionProcessor>().StartActions();
                    eventObject = null;
                    selectPos.SetActive(false);
                }
            }
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
                transform.position = (nodePos[0] - transform.position) / (inter - count)
                    + transform.position;
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
        return 0 <= pos.x && pos.x < movableGrid.GetLength(1)
            && 0 <= pos.y && pos.y < movableGrid.GetLength(0)
            && movableGrid[pos.y, pos.x].IsMovable;
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
        var meshFactory = new Expression.Map.Hd2dCharaChipMeshFactory(3, 4);
        var dir = GetDirectionFromValue(direction);
        GetComponentInChildren<MeshFilter>().sharedMesh = meshFactory.Create(dir, patternValue);

    }

    private Expression.Map.Direction GetDirectionFromValue(int dir)
    {
        switch (dir)
        {
            case 0:
                return Expression.Map.Direction.Down;
            case 1:
                return Expression.Map.Direction.Left;
            case 2:
                return Expression.Map.Direction.Right;
            case 3:
                return Expression.Map.Direction.Up;
            default:
                return Expression.Map.Direction.Down;

        }
    }

    private void OnTriggerEnter(Collider col)
    {
        if (col.GetComponent<ActionProcessor>()!=null
            && col.GetComponent<ActionProcessor>().enabled)
        {
            eventObject = col.gameObject;
        }
    }
}
