using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

/// <summary>
/// 各種イベントコマンドとなるメソッドを定義します
/// 各種イベントは、他のイベントオブジェクトクラスのメンバにセットして利用します
/// </summary>
public class EventCommands : MonoBehaviour
{
    GameObject canvas;
    List<GameObject> choiceButtons;//選択肢リスト
    GameObject buttonOrigin;//複製用ボタン
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

    // Use this for initialization
    void Start()
    {
        canvas = GameObject.Find("Canvas");
        choiceButtons = new List<GameObject>();
        buttonOrigin = canvas.transform.FindChild("Button Origin").gameObject;
        eventDic = new Dictionary<string, int>();
        eventDic.Add("メッセージ", 0);
        eventDic.Add("選択肢", 1);
        eventDic.Add("分岐終点", 2);
    }

    // Update is called once per frame
    void Update()
    {

    }

    /// <summary>
    /// メッセージ描画
    /// </summary>
    public void WriteMessage(string message)
    {
        GameObject messageBox = canvas.transform.FindChild("Message Box").gameObject;
        GameObject text = messageBox.transform.FindChild("Text").gameObject;
        text.GetComponent<Text>().text = message;
        messageBox.SetActive(true);
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

    public void MakeChoices(params string[] choices)
    {
        choiceButtons = new List<GameObject>();
        choiceName = "";
        GameObject choiceBox = canvas.transform.FindChild("Choice Box").gameObject;//選択肢ウィンドウ
        choiceBox.SetActive(true);
        int textSizeY = 140;
        int margin = 30;
        choiceBox.GetComponent<RectTransform>().localPosition = new Vector2(230, -420 + (textSizeY / 2) * choices.GetLength(0));
        choiceBox.GetComponent<RectTransform>().sizeDelta = new Vector2(600, margin * 2 + textSizeY * choices.GetLength(0));
        for (int i = 0; i < choices.GetLength(0); i++)
        {
            choiceButtons.Add(Instantiate(buttonOrigin));
            choiceButtons[i].GetComponent<Text>().text = choices[i];
            choiceButtons[i].transform.SetParent(choiceBox.transform);
            choiceButtons[i].SetActive(true);
            choiceButtons[i].transform.localPosition
                = new Vector2(0, choiceBox.GetComponent<RectTransform>().sizeDelta.y / 2 - margin - textSizeY * (i + 0.5f));
            choiceButtons[i].transform.localScale = Vector3.one;
            string name = choices[i];
            choiceButtons[i].GetComponent<Button>().onClick.AddListener(() => SetChoice(/*choices[i]*/name));
        }
        choiceName = "";
        isCompleted = true;
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
    /// </summary>
    public void SetChoice(string choiceName)
    {
        this.choiceName = choiceName;
    }

    public void BuyItem()
    {
        int itemNo = 0;
        for (int i = 0; i < Data.items.Count; i++)
        {
            if (Data.items[i].name == choiceName)
            {
                itemNo = i;
                break;
            }
        }
        if (PlayerData.money > Data.items[itemNo].price)
        {
            Data.items[itemNo].possessionCount++;
            PlayerData.money -= Data.items[itemNo].price;
        }

    }
}
