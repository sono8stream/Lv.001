using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class EventObject : MonoBehaviour
{

    [SerializeField]
    TextAsset scriptText;//スクリプトファイル
    [SerializeField]
    int[,] mapdata;
    [SerializeField]
    string[] scripts;
    int line;//現在読んでいるスクリプトの行数
    EventCommands eventCommands;
    List<UnityEvent> events;
    bool canThrough;
    public bool CanThrough
    {
        get { return canThrough; }
        set { canThrough = value; }
    }

    // Use this for initialization
    void Start()
    {
        line = 0;
        char[] kugiri = { '\r' };
        scripts = scriptText.text.Split(kugiri);
        //eventCommands = Resources.Load<EventCommands>("/Scripts/EventCommands");
        eventCommands = GetComponent<EventCommands>();
        events = new List<UnityEvent>();
        eventCommands.actNo = 0;
    }

    // Update is called once per frame
    void Update()
    {

        if (events.Count > 0)
        {
            events[eventCommands.actNo].Invoke();
            if (eventCommands.IsCompleted)
            {
                eventCommands.IsCompleted = false;
                eventCommands.actNo++;
                Debug.Log(eventCommands.actNo);
                if (eventCommands.actNo == events.Count)
                {
                    line++;
                    ReadScript();
                }
            }
        }
    }

    public void ReadScript()
    {
        events = new List<UnityEvent>();
        if (line == scripts.GetLength(0))
        {
            line = 0;
            EventCommands.isProcessing = false;
            return;
        }
        eventCommands.actNo = 0;
        string command = scripts[line].Substring(scripts[line].IndexOf("{") + 1,
            scripts[line].IndexOf("}") - scripts[line].IndexOf("{") - 1);//コマンド名を取得
        string param1 = "";
        if (scripts[line].Contains("("))
        {
            param1 = scripts[line].Substring(scripts[line].IndexOf("(") + 1,
                scripts[line].IndexOf(")") - scripts[line].IndexOf("(") - 1);//引数を取得
        }
        string[] properties;
        switch (eventCommands.eventDic[command])
        {
            case 0://メッセージ描画
                events.Add(new UnityEvent());
                events[events.Count - 1].AddListener(() => eventCommands.WriteMessage(param1));
                events.Add(new UnityEvent());
                events[events.Count - 1].AddListener(() => eventCommands.WaitForInput());
                events.Add(new UnityEvent());
                events[events.Count - 1].AddListener(() => eventCommands.CloseMessage());
                break;
            case 1://選択肢
                properties = param1.Split('|');//一文字目はメッセージ、それ以降は選択肢
                events.Add(new UnityEvent());
                events[events.Count - 1].AddListener(() => eventCommands.WriteMessage(properties[0]));
                events.Add(new UnityEvent());
                events[events.Count - 1].AddListener(()
                    => eventCommands.MakeChoices(false, properties.Skip(1).Take(properties.GetLength(0) - 1).ToArray()));
                events.Add(new UnityEvent());
                events[events.Count - 1].AddListener(() => eventCommands.WaitForChoosing());
                events.Add(new UnityEvent());
                events[events.Count - 1].AddListener(() => eventCommands.CloseMessage());
                events.Add(new UnityEvent());
                events[events.Count - 1].AddListener(() => eventCommands.CloseChoices());
                events.Add(new UnityEvent());
                events[events.Count - 1].AddListener(() => JumpCommand());
                break;
            case 2://買い物
                properties = param1.Split('|');//一文字目はメッセージ、それ以降は選択肢
                string[] branch = new string[3] { "買う", "売る", "さようなら" };
                events.Add(new UnityEvent());
                events[events.Count - 1].AddListener(() => eventCommands.WriteMessage(properties[0]));
                events.Add(new UnityEvent());
                events[events.Count - 1].AddListener(() => eventCommands.MakeChoices(false, branch));
                events.Add(new UnityEvent());
                events[events.Count - 1].AddListener(() => eventCommands.WaitForChoosing());
                events.Add(new UnityEvent());
                events[events.Count - 1].AddListener(() => eventCommands.CloseChoices());
                events.Add(new UnityEvent());
                events[events.Count - 1].AddListener(() => eventCommands.CloseMessage());
                events.Add(new UnityEvent());
                events[events.Count - 1].AddListener(() => eventCommands.SetBranch(branch, new int[3] { 1, 5, 9 }));
                events.Add(new UnityEvent());//以下、買う処理
                events[events.Count - 1].AddListener(() =>
                  eventCommands.MakeChoices(true, properties.Skip(1).Take(properties.GetLength(0) - 1).ToArray()));
                events.Add(new UnityEvent());
                events[events.Count - 1].AddListener(() => eventCommands.WriteMessage(PlayerData.money.ToString() + "G",
                    -100, 800, 800, 200));
                events.Add(new UnityEvent());
                events[events.Count - 1].AddListener(() => eventCommands.WaitForChoosing());
                events.Add(new UnityEvent());
                events[events.Count - 1].AddListener(() => eventCommands.BuyOrSellItem(true));
                events.Add(new UnityEvent());//以下、売る処理
                events[events.Count - 1].AddListener(() => eventCommands.ChoiceSellItem());
                events.Add(new UnityEvent());
                events[events.Count - 1].AddListener(() => eventCommands.WriteMessage(PlayerData.money.ToString() + "G",
                    -100, 800, 800, 200));
                events.Add(new UnityEvent());
                events[events.Count - 1].AddListener(() => eventCommands.WaitForChoosing());
                events.Add(new UnityEvent());
                events[events.Count - 1].AddListener(() => eventCommands.BuyOrSellItem(false));
                events.Add(new UnityEvent());
                events[events.Count - 1].AddListener(() => eventCommands.WriteMessage("おおきに!"));
                events.Add(new UnityEvent());
                events[events.Count - 1].AddListener(() => eventCommands.WaitForInput());
                events.Add(new UnityEvent());
                events[events.Count - 1].AddListener(() => eventCommands.CloseMessage());
                break;
            case 3:
                properties = param1.Split('|');//一文字目はファイル名、それ以降はrect
                Sprite sprite = Resources.Load<Sprite>("Sprites/" + properties[0]);
                events.Add(new UnityEvent());
                events[events.Count - 1].AddListener(() => eventCommands.DrawImage(sprite,
                    new Rect(float.Parse(properties[1]), float.Parse(properties[2]),
                    float.Parse(properties[3]), float.Parse(properties[4]))));
                break;
            case 4://分岐終点
                events.Add(new UnityEvent());
                string s = "選択肢終点";
                events[events.Count - 1].AddListener(() => JumpCommand(s));
                break;
        }
        //EventCommands.isProcessing = true;
    }

    void JumpCommand(string labelS = null)
    {
        string label;
        if (labelS == null)
        {
            label = eventCommands.ChoiceName;
        }
        else
        {
            label = labelS;
        }
        for (int i = line + 1; i < scripts.GetLength(0); i++)
        {
            if (scripts[i].Contains(label))
            {
                line = i;
                eventCommands.IsCompleted = true;
                break;
            }
        }
    }
}
