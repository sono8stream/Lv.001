﻿using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.UI;

/// <summary>
/// 各種アクションを実行するためのゲーム内のコンポーネントを保持します
/// 【暫定】アクションをクラスに切り出し中。
/// </summary>
public class ActionEnvironment : MonoBehaviour
{
    public GameObject canvas;
    public List<GameObject> choiceBoxes;
    public List<GameObject> choiceButtons;//選択肢リスト
    public List<GameObject> windows;//メッセージボックスリスト
    List<GameObject> images;//画像リスト
    public string ChoiceName { get; set; }
    public string ChoiceNameSub { get; set; }

    string subParam;//選択肢利用時の一時保持用
    bool isCompleted;//イベントが終了したか
    public bool IsCompleted
    {
        get { return isCompleted; }
        set { isCompleted = value; }
    }
    public Dictionary<string, int> eventDic;
    public int actNo;//処理中のコマンド内での処理の番号
    bool isSelecting;
    [SerializeField]
    int[] selfVar = new int[10] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    public int[] SelfVar
    {
        get { return selfVar; }
        set { selfVar = value; }
    }

    public ActionProcessor Processor { get; private set; }
    public UI.Map.Hd2dMap Map { get; private set; }

    // Use this for initialization
    void Start()
    {
        canvas = GameObject.Find("Canvas");
        choiceBoxes = new List<GameObject>();
        choiceButtons = new List<GameObject>();
        windows = new List<GameObject>();
        eventDic = new Dictionary<string, int>();
        eventDic.Add("メッセージ", 0);
        eventDic.Add("選択肢", 1);
        eventDic.Add("買い物", 2);
        eventDic.Add("画像表示", 3);
        eventDic.Add("メニュー", 4);
        eventDic.Add("移動", 5);
        eventDic.Add("条件分岐", 6);
        eventDic.Add("勧誘", 7);
        eventDic.Add("パーティ追加", 8);
        eventDic.Add("パーティ変更", 9);
        eventDic.Add("分岐終点", 10);
        eventDic.Add("セーブロード", 11);
        eventDic.Add("ゲーム終了", 12);
        eventDic.Add("フラグ", 13);
        if (selfVar[0] == 1)
        {
            GetComponent<ActionProcessor>().enabled = false;
            GetComponent<SpriteRenderer>().enabled = false;
        }
        Processor = GetComponent<ActionProcessor>();
        Map = GameObject.Find("Map").GetComponent<UI.Map.Hd2dMap>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    /// <summary>
    /// メッセージを描画
    /// </summary>
    /// <param name="message"></param>
    /// <param name="x">表示位置x座標</param>
    /// <param name="y">表示位置y座標</param>
    /// <param name="width">メッセージウィンドウ横幅</param>
    /// <param name="height">メッセージウィンドウ縦幅</param>
    public void WriteMessage(string message, float x = 0, float y = -690, float width = 1060, float height = 500)
    {
        isCompleted = true;
        Debug.Log(message);
        if (message.Equals(""))
        {
            return;
        }
        GameObject messageBox = canvas.transform.Find("Message Box").gameObject;
        windows.Add(Instantiate(messageBox));
        GameObject win = windows[windows.Count - 1];
        GameObject text = win.transform.Find("Text").gameObject;
        text.GetComponent<Text>().text = message;
        win.transform.SetParent(canvas.transform);
        win.GetComponent<RectTransform>().localScale = Vector3.one;
        win.SetActive(true);
        win.GetComponent<RectTransform>().anchoredPosition = new Vector2(x, y);
        win.GetComponent<RectTransform>().sizeDelta = new Vector2(width, height);
        text.GetComponent<RectTransform>().sizeDelta = new Vector2(width - 100, height - 50);
        text.SetActive(true);
    }

    public void WriteStatus()
    {
        if (CheckMenuCommand("装備", "道具", "装備", "セーブ", "閉じる"))
        {
            return;
        }
        GameObject[] choices = new GameObject[PlayerData.Instance.party.Count];
        for (int i = 0; i < choices.Length; i++)
        {
            string status;
            int pow = PlayerData.Instance.party[i].weapon.name.Equals("--") ? 0 :
                PlayerData.Instance.party[i].weapon.param;
            int jobNo = PlayerData.Instance.party[i].weapon.name.Equals("勇者の聖杯") ? (int)JobType.勇者
                : PlayerData.Instance.party[i].status[(int)StatusParams.skillNo];
            status = PlayerData.Instance.party[i].name + "  "
            + (JobType)jobNo;
            status += "\r\nLv:  " + (PlayerData.Instance.party[i].status[(int)StatusParams.Lv] + pow).ToString()
                + " HP:  " + (PlayerData.Instance.party[i].status[(int)StatusParams.HP] + pow / 2).ToString()
                + "\r\nスキル:　" + (SkillType)jobNo;
            WriteMessage(status, 130, 450 - 370 * i, 750, 400);
            choices[i] = windows[windows.Count - 1];
        }
        MakeChoicesPlus(false, new Vector2(130, -155), 800, 1480, choices);
        isCompleted = true;
    }

    public void WriteStatusOne(int charaNo, bool kanyu = true)
    {
        string status;
        int pow = PlayerData.Instance.characters[charaNo].weapon.name.Equals("--") ? 0 :
            PlayerData.Instance.characters[charaNo].weapon.param;
        int jobNo = PlayerData.Instance.characters[charaNo].weapon.name.Equals("勇者の聖杯") ? (int)JobType.勇者
            : PlayerData.Instance.characters[charaNo].status[(int)StatusParams.skillNo];
        status = PlayerData.Instance.characters[charaNo].name + "  "
            + (JobType)jobNo;
        status += "\r\nLv:  " + (PlayerData.Instance.characters[charaNo].status[(int)StatusParams.Lv]+pow).ToString()
            + "\r\nスキル:　" + (SkillType)jobNo;
        WriteMessage(status, -40, 350, 950, 400);
        if (kanyu)
        {
            int trust = 0;
            for (int i = 0; i < PlayerData.Instance.party.Count; i++)
            {
                trust += PlayerData.Instance.party[i].status[(int)StatusParams.Lv];
                if (PlayerData.Instance.party[i].weapon.name != ("--"))
                {
                    trust += PlayerData.Instance.party[i].weapon.param;
                }
            }
            WriteMessage("信頼度:  " + trust.ToString(), -220, 800, 600, 150);
        }
        isCompleted = true;
    }

    public void AddParty(int charaNo)
    {
        PlayerData.Instance.party.Add(PlayerData.Instance.characters[charaNo]);
        SetSelfVar();
        if (!PlayerData.Instance.characters[charaNo].onceFriend)
        {
            PlayerData.Instance.money
                += (int)Math.Pow(PlayerData.Instance.characters[charaNo].status[(int)StatusParams.Lv], 2);
        }
        PlayerData.Instance.characters[charaNo].onceFriend = true;
        isCompleted = true;
    }

    public void ChangeParty(int charaNo)
    {
        if (!ChoiceName.Equals("やめる"))
        {
            int motoNo = 0;
            for (int i = 0; i < PlayerData.Instance.party.Count; i++)
            {
                if (PlayerData.Instance.party[i].name.Equals(ChoiceName))
                {
                    motoNo = i;
                    break;
                }
            }
            int motoCNo = 0;
            for (int i = 0; i < PlayerData.Instance.characters.Count; i++)
            {
                if (PlayerData.Instance.characters[i].name.Equals(PlayerData.Instance.party[motoNo].name))
                {
                    motoCNo = i;
                    break;
                }
            }
            if (!ChoiceNameSub.Equals(""))
            {
                CloseMessage();//説明消す
            }
            if (ChoiceName.Equals(ChoiceNameSub))//2度選択
            {
                PlayerData.Instance.party[motoNo] = PlayerData.Instance.characters[charaNo];
                CloseMessage(true);
                WriteMessage("仲間の変更を行いました");
                SetSelfVar();
                if (!PlayerData.Instance.characters[charaNo].onceFriend)
                {
                    PlayerData.Instance.money
                        += (int)Math.Pow(PlayerData.Instance.characters[charaNo].status[(int)StatusParams.Lv], 2);
                }
                PlayerData.Instance.characters[charaNo].onceFriend = true;
                GameObject g = GameObject.Find("Character" + (motoCNo + 1).ToString());
                g.GetComponent<ActionEnvironment>().SelfVar[0] = 0;
                isCompleted = true;
            }
            else
            {
                ChoiceNameSub = string.Copy(ChoiceName);
                ChoiceName = "";
                WriteStatusOne(motoCNo,false);
                JumpAction(-1);
            }
        }
        else
        {
            CloseMessage(true);
            WriteMessage("仲間の変更をやめました");
        isCompleted = true;
        }
    }

    /// <summary>
    /// より詳細なステータスを表示し、ステータスを入れ替える選択肢を表示
    /// </summary>
    public void SetWeapon()
    {
        if (CheckMenuCommand("装備", "道具", "装備", "セーブ", "閉じる"))
        {
            return;
        }
        CloseChoices();
        CloseMessage(true);
        WriteMessage(PlayerData.Instance.money.ToString() + "G", 220, 860, 600, 150);
        string status;
        int unitNo = int.Parse(ChoiceName);
        int pow = PlayerData.Instance.party[unitNo].weapon.name.Equals("--") ? 0 :
            PlayerData.Instance.party[unitNo].weapon.param;
        int jobNo = PlayerData.Instance.party[unitNo].weapon.name.Equals("勇者の聖杯") ? (int)JobType.勇者
            : PlayerData.Instance.party[unitNo].status[(int)StatusParams.skillNo];
        status = PlayerData.Instance.party[unitNo].name + "  " 
            + (JobType)(jobNo);
        status += "\r\nLv:  " + (PlayerData.Instance.party[unitNo].status[(int)StatusParams.Lv]+pow).ToString()
                + " HP:  " + (PlayerData.Instance.party[unitNo].status[(int)StatusParams.HP]+pow/2).ToString()
            + "\r\nスキル:　" + (SkillType)(jobNo);
        WriteMessage(status, 150, 500, 750, 400);
        WriteMessage("装備:  " + PlayerData.Instance.party[unitNo].weapon.name, 150, 200, 600, 150);
        WriteMessage(PlayerData.Instance.party[unitNo].weapon.exp);
        ChoiceHaveItem(false, (int)ItemType.武器);
        /*GameObject[] choices = new GameObject[PlayerData.Instance.party.Count];
        for (int i = 0; i < choices.Length; i++)
        {
            string status;
            status = PlayerData.Instance.party[i].name + "  " + (JobType)(PlayerData.Instance.party[i].skillNo);
            status += "\r\nHP:  " + PlayerData.Instance.party[i].HP.ToString() + "\r\n" + (SkillType)(PlayerData.Instance.party[i].skillNo);
            WriteMessage(status, 230, 600 - 370 * i, 600, 350);
            choices[i] = windows[windows.Count - 1];
        }
        MakeChoicesPlus(false, new Vector2(230, -155), 600, 1480, choices);*/
        isCompleted = true;
    }

    public void EquipWeapon()
    {
        if(CheckMenuCommand("装備", "道具", "装備", "セーブ", "閉じる"))
        {
            return;
        }
        int unitNo = int.Parse(subParam);
        if (unitNo == 0)
        {
            Debug.Log(subParam);
            int itemNo = 0;
            for (int i = 0; i < Data.Instance.items.Count; i++)
            {
                if (Data.Instance.items[i].name.Equals(ChoiceName))
                {
                    itemNo = i;
                    break;
                }
            }
            PlayerData.Instance.party[unitNo].weapon = Data.Instance.items[itemNo];
        }
        ChoiceName = subParam;
        isCompleted = true;
    }

    public void CloseMessage(bool all = false,bool once=false)
    {
        if (all)
        {
            if (windows.Count > 0)
            {
                for (int i = 0; i < windows.Count; i++)
                {
                    Destroy(windows[i]);
                }
                windows = new List<GameObject>();
            }
        }
        else
        {
            int winNo = windows.Count - 1;
            if (winNo >= 0)
            {
                GameObject sub = windows[winNo];
                windows.RemoveAt(winNo);
                Destroy(sub);
            }
        }
        if(once)
        {
            SetSelfVar();
        }
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
    public void MakeChoices(bool isShopping,Vector2 defaultPos ,float length,float height, params string[] choices)
    {
        choiceButtons = new List<GameObject>();
        ChoiceName = "";
        ChoiceNameSub = "";
        GameObject choiceBox = canvas.transform.Find("Choice Box").gameObject;//選択肢ウィンドウ
        choiceBoxes.Add(Instantiate(choiceBox));
        int boxNo = choiceBoxes.Count - 1;
        choiceBoxes[boxNo].transform.SetParent(canvas.transform);
        choiceBoxes[boxNo].transform.localScale = Vector3.one;
        choiceBoxes[boxNo].SetActive(true);
        GameObject selectBranch = choiceBoxes[boxNo].transform.Find("Select Branch").gameObject;
        selectBranch.SetActive(false);
        selectBranch.GetComponent<RectTransform>().sizeDelta = new Vector2(length, 100);
        int textSizeY = 100;
        int margin = 30;
        if (defaultPos == Vector2.zero)
        {
            defaultPos = new Vector2(30, -420);
        }
        int commandCount = choices.Length;
        if(isShopping)
        {
            commandCount++;
        }
        if (height == -1)
        {
            height = textSizeY * commandCount;
        }
        choiceBoxes[boxNo].GetComponent<RectTransform>().localPosition
            = new Vector2(defaultPos.x, defaultPos.y + (textSizeY / 2) * commandCount);
        choiceBoxes[boxNo].GetComponent < RectTransform>().sizeDelta = new Vector2(length, margin * 2 + /*textSizeY * commandCount*/height);
        EventTrigger trigger;//イベント
        EventTrigger.Entry entry;
        GameObject buttonOrigin = canvas.transform.Find("Button Origin").gameObject;
        for (int i = 0; i < commandCount; i++)
        {
            choiceButtons.Add(Instantiate(buttonOrigin));
            string t, s;
            if (isShopping)
            {
                if (i < commandCount - 1)
                {
                    t = Data.Instance.items[int.Parse(choices[i])].name;
                    s = Data.Instance.items[int.Parse(choices[i])].price.ToString() + "G";
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
            int no = i;
            choiceButtons[i].GetComponent<Text>().text = t;
            Transform text = choiceButtons[i].transform.Find("Text");
                text.GetComponent<Text>().text = s;
            choiceButtons[i].transform.SetParent(choiceBoxes[boxNo].transform);
            choiceButtons[i].SetActive(true);
            choiceButtons[i].transform.localPosition
                = new Vector2(0, /*choiceBoxes[boxNo].GetComponent<RectTransform>().sizeDelta.y / 2 - margin - textSizeY * (i + 0.5f)*/
                height / 2 - height / commandCount * (i+0.5f));
            choiceButtons[i].GetComponent<RectTransform>().sizeDelta = new Vector2(length - 50, textSizeY);
            text.GetComponent<RectTransform>().sizeDelta = choiceButtons[i].GetComponent<RectTransform>().sizeDelta;
            choiceButtons[i].transform.localScale = Vector3.one;
            choiceButtons[i].GetComponent<Button>().onClick.AddListener(() => SetChoice(t));
            trigger = choiceButtons[i].GetComponent<EventTrigger>();
            trigger.triggers = new List<EventTrigger.Entry>();
            entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerDown;    //イベントのタイプSelectが発生した際に
            Vector2 pos= choiceButtons[no].GetComponent<RectTransform>().anchoredPosition;
            entry.callback.AddListener((x) => SetSelectBranch(pos,selectBranch));
            trigger.triggers.Add(entry);
        }
        ChoiceName = "";
        isCompleted = true;
    }

    /// <summary>
    /// 選択肢生成
    /// </summary>
    /// <param name="isShopping"></param>
    /// <param name="choices">選択肢の名前,isShoppingならばアイテムの番号</param>
    public void MakeChoicesPlus(bool isShopping, Vector2 defaultPos, float length, float height, GameObject[] choices)
    {
        ChoiceName = "";
        ChoiceNameSub = "";
        GameObject choiceBox = canvas.transform.Find("Choice Box").gameObject;//選択肢ウィンドウ
        choiceBoxes.Add(Instantiate(choiceBox));
        int boxNo = choiceBoxes.Count - 1;
        choiceBoxes[boxNo].transform.SetParent(canvas.transform);
        choiceBoxes[boxNo].transform.localScale = Vector3.one;
        choiceBoxes[boxNo].SetActive(true);
        GameObject selectBranch = choiceBoxes[boxNo].transform.Find("Select Branch").gameObject;
        selectBranch.SetActive(false);
        selectBranch.GetComponent<RectTransform>().sizeDelta = choices[0].GetComponent<RectTransform>().sizeDelta;
        int textSizeY = 100;
        int margin = 30;
        if (defaultPos == Vector2.zero)
        {
            defaultPos = new Vector2(30, -420);
        }
        int commandCount = choices.Length;
        if (isShopping)
        {
            commandCount++;
        }
        if (height == -1)
        {
            height = textSizeY * commandCount + margin * 2;
        }
        choiceBoxes[boxNo].GetComponent<RectTransform>().localPosition
            = new Vector2(defaultPos.x, defaultPos.y + (textSizeY / 2) * commandCount);
        choiceBoxes[boxNo].GetComponent<RectTransform>().sizeDelta = new Vector2(length, margin * 2 + /*textSizeY * commandCount*/height);
        EventTrigger trigger;//イベント
        EventTrigger.Entry entry;
        for (int i = 0; i < commandCount; i++)
        {
            choices[i].transform.SetParent(choiceBoxes[boxNo].transform);
            trigger = choices[i].AddComponent<EventTrigger>();
            trigger.triggers = new List<EventTrigger.Entry>();
            entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerDown;    //イベントのタイプSelectが発生した際に
            int no = i;
            Vector2 pos = choices[no].GetComponent<RectTransform>().anchoredPosition;
            entry.callback.AddListener((x) => SetSelectBranch(pos, selectBranch));
            trigger.triggers.Add(entry);
            entry.eventID = EventTriggerType.PointerClick;
            entry.callback.AddListener((x) => SetChoice(no.ToString()));
        }
        selectBranch.transform.SetSiblingIndex(commandCount);
        ChoiceName = "";
        isCompleted = true;
    }

    /// <summary>
    /// 売却するアイテムの候補を選択肢表示
    /// </summary>
    public void ChoiceHaveItem(bool isShopping, int itemTypeNo = -1)
    {
        List<string> posItems = new List<string>();//所持アイテム
        for (int i = 0; i < Data.Instance.items.Count; i++)
        {
            if (Data.Instance.items[i].possessionCount > 0 && (itemTypeNo == -1 || Data.Instance.items[i].type == itemTypeNo))
            {
                string text;
                if(isShopping)
                {
                    text = i.ToString();
                }
                else
                {
                    text = Data.Instance.items[i].name;
                }
                posItems.Add(text);
            }
        }
        if (!isShopping && itemTypeNo == (int)ItemType.武器)
        {
            posItems.Add("--");
        }
        float x = isShopping ? 30 : 200;
        float length = isShopping ? 1000 : 600;
        MakeChoices(isShopping, new Vector2(x, -400), length, -1, posItems.ToArray());
    }

    /// <summary>
    /// メニューコマンドをクリックしたかチェック
    /// </summary>
    public bool CheckMenuCommand(string nowCommand, params string[] commands)
    {
        bool isMenu = false;
        Debug.Log(ChoiceName);
        for (int i = 0; i < commands.Length; i++)
        {
            isMenu |= (ChoiceName.Equals(commands[i]) /*&& !ChoiceName.Equals(nowCommand)*/);
        }
        if (isMenu)
        {
            CloseChoices();
            CloseMessage(true);
            WriteMessage(PlayerData.Instance.money.ToString() + "G", 220, 860, 600, 150);            
            JumpAction(2, false);
        }
        Debug.Log(isMenu);
        return isMenu;
    }

    public void CloseChoices(bool all = false)
    {
        if (all)
        {
            if (choiceBoxes.Count > 0)
            {
                for (int i = 0; i < choiceBoxes.Count; i++)
                {
                    Destroy(choiceBoxes[i]);
                }
                choiceBoxes = new List<GameObject>();
            }
        }
        else
        {
            int boxNo = choiceBoxes.Count - 1;
            if (boxNo >= 0)
            {
                GameObject sub = choiceBoxes[boxNo];
                choiceBoxes.RemoveAt(boxNo);
                Destroy(sub);
            }
        }
        isCompleted = true;
    }

    public void WaitForChoosing()//選択肢の選択待ち
    {
        isCompleted = !ChoiceName.Equals("");
    }

    /// <summary>
    /// 選択した選択肢をセット
    /// buttonのonclickに付与
    /// 原則2連続タップだが、
    /// onceは一回選択で遷移可能なケース
    /// </summary>
    public void SetChoice(string choiceName)
    {
        this.ChoiceName = choiceName;
    }

    public void SaveChoice()
    {
        subParam = ChoiceName;
        isCompleted = true;
    }

    public void SetSelectBranch(Vector2 pos,GameObject branch)
    {
        branch.SetActive(true);
        branch.GetComponent<RectTransform>().anchoredPosition = pos;
        Debug.Log(pos);
    }

    /// <summary>
    /// ChoiceNameと同名のアイテムを購入
    /// </summary>
    public void BuyOrSellItem(bool buy)
    {
        if (!ChoiceName.Equals("終了"))
        {
            int itemNo = 0;
            for (int i = 0; i < Data.Instance.items.Count; i++)
            {
                if (Data.Instance.items[i].name.Equals(ChoiceName))
                {
                    itemNo = i;
                    break;
                }
            }
            if (!ChoiceName.Equals(ChoiceNameSub))//何も選択していない
            {
                if (!ChoiceNameSub.Equals(""))
                {
                    CloseMessage();//説明消す
                    CloseMessage();//所持数消す
                }
                ChoiceNameSub = string.Copy(ChoiceName);
                ChoiceName = "";
                WriteMessage(Data.Instance.items[itemNo].exp);
                WriteMessage("所持数:　" + Data.Instance.items[itemNo].possessionCount.ToString(),300,600,700,200);
                JumpAction(-1);
            }
            else
            {
                if (buy)
                {
                    if (PlayerData.Instance.money >= Data.Instance.items[itemNo].price)
                    {
                        Data.Instance.items[itemNo].possessionCount++;
                        PlayerData.Instance.money -= Data.Instance.items[itemNo].price;
                    }
                }
                else
                {
                    Data.Instance.items[itemNo].possessionCount--;
                    PlayerData.Instance.money += Data.Instance.items[itemNo].price;
                }
                CloseChoices();
                CloseMessage(true);
                JumpAction(-4);
            }
        }
        else
        {
            JumpAction(-99);
            isCompleted = true;
            CloseMessage(true);
            CloseChoices();
        }
    }

    public void CheckItem()
    {
        if (CheckMenuCommand("道具", "道具", "装備", "セーブ", "閉じる"))
        {
            return;
        }
        int itemNo = 0;
        for (int i = 0; i < Data.Instance.items.Count; i++)
        {
            if (Data.Instance.items[i].name.Equals(ChoiceName))
            {
                itemNo = i;
                break;
            }
        }
        CloseMessage();
        WriteMessage(Data.Instance.items[itemNo].exp);
        JumpAction(-1);
    }

    public void DrawImage(Sprite sprite, Rect rect)
    {
        GameObject imageBox = canvas.transform.Find("Image Box").gameObject;
        imageBox.SetActive(true);
        imageBox.GetComponent<Image>().sprite = sprite;
        imageBox.GetComponent<RectTransform>().anchoredPosition = rect.position;
        imageBox.GetComponent<RectTransform>().sizeDelta = rect.size;
        isCompleted = true;
    }
    public void CloseImage()
    {
        canvas.transform.Find("Image Box").gameObject.SetActive(false);
        isCompleted = true;
    }

    //条件分岐
    public void SetBoolean(string[] conditions)
    {
        int[] param = new int[2];
        for (int i = 0; i < param.Length; i++)
        {
            int no = i * 3;
            if (conditions[no].Equals("セルフ変数"))
            {
                param[i] = selfVar[int.Parse(conditions[no + 1])];
            }
            else if (conditions[no].Equals("キャラ"))
            {
                param[i] = PlayerData.Instance.characters[int.Parse(conditions[no + 1])]
                    .status[int.Parse(conditions[no + 2])];
            }
            else if (conditions[no].Equals("パーティ人数"))
            {
                param[i] = PlayerData.Instance.party.Count;
            }
            else if (conditions[no].Equals("信頼度"))
            {
                param[i] = 0;
                for (int j = 0; j < PlayerData.Instance.party.Count; j++)
                {
                    param[i] += PlayerData.Instance.party[j].status[(int)StatusParams.Lv];
                    if (PlayerData.Instance.party[j].weapon.name != ("--"))
                    {
                        param[i] += PlayerData.Instance.party[j].weapon.param;
                    }
                }
                param[i] += int.Parse(conditions[no + 1]);
            }
            else
            {
                param[i] = int.Parse(conditions[no]);
            }
        }
        if (conditions[conditions.Length - 1].Equals(">"))
        {
            ChoiceName = param[0] > param[1] ? "true" : "false";
        }
        if (conditions[conditions.Length - 1].Equals("="))
        {
            ChoiceName = param[0] == param[1] ? "true" : "false";
        }
        if (conditions[conditions.Length - 1].Equals("<"))
        {
            ChoiceName = param[0] < param[1] ? "true" : "false";
        }
        Debug.Log(param[0]);
        Debug.Log(param[1]);
        Debug.Log(ChoiceName);
        isCompleted = true;
    }

    public void SetSelfVar()
    {
        selfVar[0] = 1;
        //isCompleted = true;
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
            if(ChoiceName.Equals(destNames[i]))
            {
                JumpAction(destNos[i]);
            }
        }
        isCompleted = true;
        ChoiceName = "";
    }

    public void Move(int sceneNo,Vector2 pos)
    {
        PlayerData.Instance.pos = pos;
        PlayerData.Instance.SaveSelfVars(SceneManager.GetActiveScene().buildIndex);
        SceneManager.LoadScene(sceneNo);
    }

    public void SaveLoadData(bool canSave)
    {
        Debug.Log("okashii");
        if(canSave)
        {
            PlayerData.Instance.Save();
        }
        else
        {
            Debug.Log("i'll load");
            PlayerData.Instance.Load();
        }
        isCompleted = true;
        Debug.Log("comp!!");
    }

    /// <summary>
    /// 1コマンド内での処理ジャンプ
    /// isCompleted=trueにより、update内でactNoが+1されるため、あらかじめ-1
    /// </summary>
    /// <param name="destNo">移動先への相対番号</param>
    public void JumpAction(int destNo, bool relative = true)
    {
        if (relative)
        {
            actNo += destNo - 1;
        }
        else
        {
            actNo = destNo - 1;
        }
        if (actNo < 0)
        {
            actNo = -1;
        }
        isCompleted = true;
    }

    public void NoOperation()
    {
        isCompleted = true;
    }


}
