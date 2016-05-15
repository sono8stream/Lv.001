using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

/// <summary>
/// 各種イベントコマンドとなるメソッドを定義します
/// 各種イベントは、他のイベントオブジェクトクラスのメンバにセットして利用します
/// </summary>
public class EventCommands : MonoBehaviour
{
    [SerializeField]
    GameObject selectBranch;
    GameObject canvas;
    List<GameObject> choiceButtons;//選択肢リスト
    List<GameObject> windows;//メッセージボックスリスト
    List<GameObject> images;//画像リスト
    string choiceName;//選択した選択肢名
    public string ChoiceName
    {
        get { return choiceName; }
        set { choiceName = value; }
    }
    bool isCompleted;//イベントが終了したか
    public bool IsCompleted
    {
        get { return isCompleted; }
        set { isCompleted = value; }
    }
    public static bool isProcessing;
    public Dictionary<string, int> eventDic;
    public int actNo;//処理中のコマンド内での処理の番号
    bool isSelecting;
    string choiceNameSub;

    // Use this for initialization
    void Start()
    {
        canvas = GameObject.Find("Canvas");
        choiceButtons = new List<GameObject>();
        eventDic = new Dictionary<string, int>();
        eventDic.Add("メッセージ", 0);
        eventDic.Add("選択肢", 1);
        eventDic.Add("買い物", 2);
        eventDic.Add("画像表示", 3);
        eventDic.Add("分岐終点", 4);
    }

    // Update is called once per frame
    void Update()
    {

    }

    /// <summary>
    /// メッセージ描画
    /// </summary>
    public void WriteMessage(string message, float x = 0, float y = -690, float width = 1060, float height = 500)
    {
        GameObject messageBox = canvas.transform.FindChild("Message Box").gameObject;
        GameObject text = messageBox.transform.FindChild("Text").gameObject;
        text.GetComponent<Text>().text = message;
        messageBox.SetActive(true);
        messageBox.GetComponent<RectTransform>().anchoredPosition = new Vector2(x, y);
        messageBox.GetComponent<RectTransform>().sizeDelta = new Vector2(width, height);
        text.GetComponent<RectTransform>().sizeDelta = new Vector2(width - 100, height - 50);
        text.SetActive(true);
        isCompleted = true;
    }

    public void CloseMessage()
    {
        GameObject messageBox = canvas.transform.FindChild("Message Box").gameObject;
        messageBox.SetActive(false);
        messageBox.transform.FindChild("Text").gameObject.SetActive(false);
        isCompleted = true;
    }

    public void WaitForInput()
    {
        isCompleted = Input.GetMouseButtonDown(0);
    }

    /// <summary>
    /// 選択肢生成
    /// </summary>
    /// <param name="isShopping"></param>
    /// <param name="choices">選択肢の名前,isShoppingならばアイテムの番号</param>
    public void MakeChoices(bool isShopping, params string[] choices)
    {
        choiceButtons = new List<GameObject>();
        choiceName = "";
        choiceNameSub = "";
        GameObject choiceBox = canvas.transform.FindChild("Choice Box").gameObject;//選択肢ウィンドウ
        choiceBox.SetActive(true);
        selectBranch = choiceBox.transform.FindChild("Select Branch").gameObject;
        selectBranch.SetActive(false);
        int textSizeY = 140;
        int margin = 30;
        Vector2 defaultPos = new Vector2(30, -420);
        int commandCount = choices.GetLength(0);
        if(isShopping)
        {
            commandCount++;
        }
        choiceBox.GetComponent<RectTransform>().localPosition
            = new Vector2(defaultPos.x, defaultPos.y + (textSizeY / 2) * commandCount);
        choiceBox.GetComponent < RectTransform>().sizeDelta = new Vector2(1000, margin * 2 + textSizeY * commandCount);
        EventTrigger trigger;//イベント
        EventTrigger.Entry entry;
        GameObject buttonOrigin = canvas.transform.FindChild("Button Origin").gameObject;
        for (int i = 0; i < commandCount; i++)
        {
            choiceButtons.Add(Instantiate(buttonOrigin));
            string t, s;
            if (isShopping)
            {
                if (i < commandCount - 1)
                {
                    t = Data.items[int.Parse(choices[i])].name;
                    s = "所持数: " + Data.items[int.Parse(choices[i])].possessionCount.ToString();
                }
                else
                {
                    t = "終了";
                    s = "";
                }
            }
            else
            {
                t = choices[i];
                s = "";
            }
            choiceButtons[i].GetComponent<Text>().text = t;
            choiceButtons[i].transform.FindChild("Text").GetComponent<Text>().text = s;
            choiceButtons[i].transform.SetParent(choiceBox.transform);
            choiceButtons[i].SetActive(true);
            choiceButtons[i].transform.localPosition
                = new Vector2(0, choiceBox.GetComponent<RectTransform>().sizeDelta.y / 2 - margin - textSizeY * (i + 0.5f));
            choiceButtons[i].transform.localScale = Vector3.one;
            choiceButtons[i].GetComponent<Button>().onClick.AddListener(() => SetChoice(t, !isShopping));
            trigger = choiceButtons[i].GetComponent<EventTrigger>();
            trigger.triggers = new List<EventTrigger.Entry>();
            entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerDown;    //イベントのタイプSelectが発生した際に
            int no = i;
            entry.callback.AddListener((x) => SetSelectBranch(choiceButtons[no].GetComponent<RectTransform>().anchoredPosition));
            trigger.triggers.Add(entry);
        }
        choiceName = "";
        isCompleted = true;
    }

    /// <summary>
    /// 売却するアイテムの候補を選択肢表示
    /// </summary>
    public void ChoiceSellItem()
    {
        List<string> posItems = new List<string>();//所持アイテム
        for (int i = 0; i < Data.items.Count; i++)
        {
            if (Data.items[i].possessionCount > 0)
            {
                posItems.Add(i.ToString());
            }
        }
        MakeChoices(true, posItems.ToArray());
    }

    public void CloseChoices()
    {
        GameObject choiceBox = canvas.transform.FindChild("Choice Box").gameObject;//選択肢ウィンドウ
        choiceBox.SetActive(false);
        while (choiceButtons.Count > 0)
        {
            Destroy(choiceButtons[0]);
            choiceButtons.RemoveAt(0);
        }
        isCompleted = true;
    }

    public void WaitForChoosing()//選択肢の選択待ち
    {
        isCompleted = !choiceName.Equals("");
    }

    /// <summary>
    /// 選択した選択肢をセット
    /// buttonのonclickに付与
    /// 原則2連続タップだが、
    /// onceは一回選択で遷移可能なケース
    /// </summary>
    public void SetChoice(string choiceName, bool once = true)
    {
        if (once || (isSelecting && choiceNameSub.Equals(choiceName)))
        {
            this.choiceName = choiceName;
            isSelecting = false;
        }
        else
        {
            choiceNameSub = choiceName;
            isSelecting = true;
        }
    }

    public void SetSelectBranch(Vector2 pos)
    {
        selectBranch.SetActive(true);
        selectBranch.GetComponent<RectTransform>().anchoredPosition
            = pos;
    }

    /// <summary>
    /// choiceNameと同名のアイテムを購入
    /// </summary>
    public void BuyOrSellItem(bool buy)
    {
        if (!choiceName.Equals("終了"))
        {
            int itemNo = 0;
            for (int i = 0; i < Data.items.Count; i++)
            {
                if (Data.items[i].name.Equals(choiceName))
                {
                    itemNo = i;
                    break;
                }
            }
            if (buy)
            {
                if (PlayerData.money > Data.items[itemNo].price)
                {
                    Data.items[itemNo].possessionCount++;
                    PlayerData.money -= Data.items[itemNo].price;
                }
            }
            else
            {
                Data.items[itemNo].possessionCount--;
                PlayerData.money += Data.items[itemNo].price;
            }
            JumpAction(-2);
        }
        else
        {
            JumpAction(-99);
            isCompleted = true;
        }
        CloseChoices();
    }

    public void DrawImage(Sprite sprite, Rect rect)
    {
        GameObject imageBox = canvas.transform.FindChild("Image Box").gameObject;
        imageBox.SetActive(true);
        imageBox.GetComponent<Image>().sprite = sprite;
        imageBox.GetComponent<RectTransform>().anchoredPosition = rect.position;
        imageBox.GetComponent<RectTransform>().sizeDelta = rect.size;
        isCompleted = true;
    }
    public void CloseImage()
    {
        canvas.transform.FindChild("Image Box").gameObject.SetActive(false);
        isCompleted = true;
    }

    /// <summary>
    /// 選択肢とそれに対応する番号のイベントアクションまで飛ぶ
    /// 移動先は相対番号
    /// </summary>
    /// <param name="destNos"></param>
    public void SetBranch(string[] destNames, int[] destNos)
    {
        for(int i=0;i<destNames.Length;i++)
        {
            if(choiceName.Equals(destNames[i]))
            {
                JumpAction(destNos[i]);
            }
        }
        isCompleted = true;
    }

    /// <summary>
    /// 1コマンド内での処理ジャンプ
    /// </summary>
    /// <param name="destNo">移動先への相対番号</param>
    public void JumpAction(int destNo)
    {
        actNo += destNo - 1;
        if(actNo<0)
        {
            actNo = 0;
        }
        isCompleted = true;
    }
}
