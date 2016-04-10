using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class EventObject : MonoBehaviour {

    [SerializeField]
    string scriptText;//スクリプトファイル
    [SerializeField]
    int[,] mapdata;
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
        scripts = scriptText.Split(kugiri);
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
                    EventCommands.isProcessing = false;
                }
            }
        }
    }

    public void ReadScript()
    {
        if(line==scripts.GetLength(0))
        {
            line = 0;
            return;
        }
        events = new List<UnityEvent>();
        string command = scripts[line].Substring(scripts[line].IndexOf("{") + 1,
            scripts[line].IndexOf("}") - scripts[line].IndexOf("{") - 1);//コマンド名を取得
        string param1= scripts[line].Substring(scripts[line].IndexOf("(") + 1,
            scripts[line].IndexOf(")") - scripts[line].IndexOf("(") - 1);//引数を取得
        switch (eventCommands.eventDic[command])
        {
            case 0://メッセージ描画
                events.Add(new UnityEvent());
                events[events.Count - 1].AddListener(() => eventCommands.WriteMessage(param1));
                events.Add(new UnityEvent());
                events[events.Count - 1].AddListener(() => eventCommands.WaitInput());
                events.Add(new UnityEvent());
                events[events.Count - 1].AddListener(() => eventCommands.CloseMessage());
                break;
        }
        EventCommands.isProcessing = true;
    }
}
