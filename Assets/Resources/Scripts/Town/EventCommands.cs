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
        eventDic = new Dictionary<string, int>();
        eventDic.Add("メッセージ", 0);
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
        GameObject messageBox = GameObject.Find("Message Box");
        GameObject text = messageBox.transform.FindChild("Text").gameObject;
        text.GetComponent<Text>().enabled = true;
        text.GetComponent<Text>().text = message;
        messageBox.GetComponent<Image>().enabled = true;
        isCompleted= true;
    }

    public void CloseMessage()
    {
        GameObject messageBox = GameObject.Find("Message Box");
        GameObject text = messageBox.transform.FindChild("Text").gameObject;
        text.GetComponent<Text>().enabled = false;
        messageBox.GetComponent<Image>().enabled = false;
        isCompleted= true;
    }

    public void WaitInput()
    {
        isCompleted = Input.GetMouseButtonDown(0);
    }
}
