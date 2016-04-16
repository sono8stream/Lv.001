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
    }

    // Update is called once per frame
    void Update()
    {

        if (events.Count > 0)
        {
            events[0].Invoke();
            if (eventCommands.IsCompleted)
            {
                eventCommands.IsCompleted = false;
                events.RemoveAt(0);
                if (events.Count == 0)
                {
                    line++;
                    ReadScript();
                }
            }
        }
    }

    public void ReadScript()
    {
        if (line == scripts.GetLength(0))
        {
            line = 0;
            EventCommands.isProcessing = false;
            return;
        }
        events = new List<UnityEvent>();
        string command = scripts[line].Substring(scripts[line].IndexOf("{") + 1,
            scripts[line].IndexOf("}") - scripts[line].IndexOf("{") - 1);//コマンド名を取得
        string param1 = "";
        if (scripts[line].Contains("("))
        {
            param1 = scripts[line].Substring(scripts[line].IndexOf("(") + 1,
                scripts[line].IndexOf(")") - scripts[line].IndexOf("(") - 1);//引数を取得
        }
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
                string[] properties = param1.Split('|');//一文字目はメッセージ、それ以降は選択肢
                events.Add(new UnityEvent());
                events[events.Count - 1].AddListener(() => eventCommands.WriteMessage(properties[0]));
                events.Add(new UnityEvent());
                events[events.Count - 1].AddListener(() 
                    => eventCommands.MakeChoices(properties.Skip(1).Take(properties.GetLength(0) - 1).ToArray()));
                events.Add(new UnityEvent());
                events[events.Count - 1].AddListener(() => eventCommands.WaitForChoosing());
                events.Add(new UnityEvent());
                events[events.Count - 1].AddListener(() => eventCommands.CloseMessage());
                events.Add(new UnityEvent());
                events[events.Count - 1].AddListener(() => eventCommands.CloseChoices());
                events.Add(new UnityEvent());
                events[events.Count - 1].AddListener(() => JumpCommand());
                break;
            case 2://分岐終点
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
