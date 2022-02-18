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
    EventCommands eventCommands;// イベントを実行するためのコマンドを保持
    List<UnityEvent> events;
    UI.Action.ActionBase currentAction;
    [SerializeField]
    bool canThrough;
    public bool CanThrough
    {
        get { return canThrough; }
        set { canThrough = value; }
    }
    [SerializeField]
    Sprite[] sprites;
    [SerializeField]
    bool isAnimating;
    int aniLimit = 60;
    int aniCount;
    int spriteCount;

    Expression.Map.MapEvent.EventData eventData;

    // Use this for initialization
    void Start()
    {
        line = 0;
        char[] kugiri = { '\n' };
        scripts = scriptText.text.Split(kugiri);
        eventCommands = GetComponent<EventCommands>();
        events = new List<UnityEvent>();
        eventCommands.actNo = 0;
        eventCommands.IsCompleted = false;
        if (gameObject.name.Contains("Character"))
        {
            Debug.Log(gameObject.name[9] - '1');
            sprites = PlayerData.Instance.characters[gameObject.name[9] - '1'].sprite;
            Debug.Log(PlayerData.Instance.characters[gameObject.name[9] - '1'].sprite);
            GetComponent<SpriteRenderer>().sprite = sprites[0];
        }
        aniCount = 0;
        spriteCount = 0;
    }

    // Update is called once per frame
    void Update()
    {
        // どう実行させようかな
        if (currentAction != null)
        {
            if (currentAction.Run())
            {
                line++;
                ReadScript2();
            }
        }

        /*
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
                    ReadScript2();
                }
            }
        }
        */
        if (isAnimating)
        {
            aniCount++;
            if (aniLimit < aniCount)
            {
                aniCount = 0;
                spriteCount++;
                if (spriteCount >= sprites.Length * 2 - 2)
                {
                    spriteCount = 0;
                }
                int spriteI = spriteCount >= sprites.Length ? sprites.Length * 2 - 2 - spriteCount : spriteCount;
                GetComponent<SpriteRenderer>().sprite = sprites[spriteI];
            }
        }
    }

    // テスト用のset関数、一時的

    public void SetScriptText(TextAsset text)
    {
        scriptText = text;
    }

    public void FetchEventCommands()
    {
        eventCommands = GetComponent<EventCommands>();
        eventCommands.Invoke("Start", 0);
    }

    // イベントコマンドを1行分読み取り実行イベントキューに入れる
    // 一旦この関数をExpression.Map.MapEvent.EventCommandDataから読み取る関数で置き換える
    // 最終的にはEventCommandDataが持つ処理を呼び出すのみにする
    public void ReadScript()
    {
        events = new List<UnityEvent>();
        if (line == scripts.GetLength(0))
        {
            line = 0;
            EventCommands.isProcessing = false;
            if (eventCommands.SelfVar[0] == 1)
            {
                GetComponent<EventObject>().enabled = false;
                GetComponent<SpriteRenderer>().enabled = false;
                Debug.Log("breaked");
            }
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
        string[] branch;
        //Debug.Log(command);
        switch (eventCommands.eventDic[command])
        {
            case 0://メッセージ描画
                string[] t = param1.Split('|');
                events.Add(new UnityEvent());
                if (t.Length == 1)
                {
                    events[events.Count - 1].AddListener(() => eventCommands.WriteMessage(t[0]));
                }
                else
                {
                    events[events.Count - 1].AddListener(() => eventCommands.WriteMessage(t[0], float.Parse(t[1]),
                        float.Parse(t[2]), float.Parse(t[3]), float.Parse(t[4])));
                }
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
                float length = float.Parse(properties[properties.Length - 1]);
                events[events.Count - 1].AddListener(()
                    => eventCommands.MakeChoices(false, Vector2.zero, length, -1, properties.Skip(1).Take(properties.GetLength(0) - 2).ToArray()));
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
                branch = new string[3] { "買う", "売る", "さようなら" };
                string st = string.Copy(properties[0]);
                float winLength = 500;
                events.Add(new UnityEvent());
                events[events.Count - 1].AddListener(() => eventCommands.WriteMessage(/*properties[0]*/st));
                events.Add(new UnityEvent());
                events[events.Count - 1].AddListener(()
                    => eventCommands.MakeChoices(false, Vector2.zero, winLength, -1, branch));
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
                  eventCommands.MakeChoices(true, Vector2.zero, 1000, -1, properties.Skip(1).Take(properties.GetLength(0) - 1).ToArray()));
                events.Add(new UnityEvent());
                events[events.Count - 1].AddListener(() => eventCommands.WriteMessage(PlayerData.Instance.money.ToString() + "G",
                    -100, 860, 400, 150));
                events.Add(new UnityEvent());
                events[events.Count - 1].AddListener(() => eventCommands.WaitForChoosing());
                events.Add(new UnityEvent());
                events[events.Count - 1].AddListener(() => eventCommands.BuyOrSellItem(true));
                events.Add(new UnityEvent());//以下、売る処理
                events[events.Count - 1].AddListener(() => eventCommands.ChoiceHaveItem(true));
                events.Add(new UnityEvent());
                events[events.Count - 1].AddListener(() => eventCommands.WriteMessage(PlayerData.Instance.money.ToString() + "G",
                    -100, 860, 600, 150));
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
            case 3://画像表示
                properties = param1.Split('|');//一文字目はファイル名、それ以降はrect
                Sprite sprite = Resources.Load<Sprite>("Sprites/" + properties[0]);
                events.Add(new UnityEvent());
                events[events.Count - 1].AddListener(() => eventCommands.DrawImage(sprite,
                    new Rect(float.Parse(properties[1]), float.Parse(properties[2]),
                    float.Parse(properties[3]), float.Parse(properties[4]))));
                break;
            case 4://メニュー
                branch = new string[4] { "道具", "装備", "セーブ", "閉じる" };
                Vector2 v = new Vector2(-380, -400);
                events.Add(new UnityEvent());
                events[events.Count - 1].AddListener(() => eventCommands.MakeChoices(false, v, 300, -1, branch));
                events.Add(new UnityEvent());
                events[events.Count - 1].AddListener(() => eventCommands.WriteMessage(PlayerData.Instance.money.ToString() + "G",
                    220, 860, 600, 150));
                events.Add(new UnityEvent());
                events[events.Count - 1].AddListener(() => eventCommands.WaitForChoosing());
                events.Add(new UnityEvent());
                events[events.Count - 1].AddListener(() => eventCommands.SetBranch(branch, new int[4] { 1, 5, 12, 13 }));
                events.Add(new UnityEvent());//以下、"道具"コマンド処理
                events[events.Count - 1].AddListener(() => eventCommands.ChoiceHaveItem(false));
                events.Add(new UnityEvent());
                events[events.Count - 1].AddListener(() => eventCommands.WaitForChoosing());
                events.Add(new UnityEvent());
                events[events.Count - 1].AddListener(() => eventCommands.CheckItem());
                events.Add(new UnityEvent());
                events[events.Count - 1].AddListener(() => eventCommands.JumpAction(-3));
                events.Add(new UnityEvent());//以下、"装備"コマンド処理
                events[events.Count - 1].AddListener(() => eventCommands.WriteStatus());
                events.Add(new UnityEvent());
                events[events.Count - 1].AddListener(() => eventCommands.WaitForChoosing());
                events.Add(new UnityEvent());
                events[events.Count - 1].AddListener(() => eventCommands.SaveChoice());
                events.Add(new UnityEvent());
                events[events.Count - 1].AddListener(() => eventCommands.SetWeapon());
                events.Add(new UnityEvent());
                events[events.Count - 1].AddListener(() => eventCommands.WaitForChoosing());
                events.Add(new UnityEvent());
                events[events.Count - 1].AddListener(() => eventCommands.EquipWeapon());
                events.Add(new UnityEvent());
                events[events.Count - 1].AddListener(() => eventCommands.JumpAction(-3));
                events.Add(new UnityEvent());//以下、"中断"処理
                events[events.Count - 1].AddListener(() => eventCommands.SaveLoadData(true));
                //events.Add(new UnityEvent());
                //events[events.Count - 1].AddListener(() => Application.Quit());
                events.Add(new UnityEvent());//以下、"閉じる"処理
                events[events.Count - 1].AddListener(() => eventCommands.CloseChoices(true));
                events.Add(new UnityEvent());
                events[events.Count - 1].AddListener(() => eventCommands.CloseMessage(true));
                break;
            case 5://移動
                properties = param1.Split('|');//一文字目は移動先、それ以降は座標
                int sceneNo = int.Parse(properties[0]);
                Vector2 pos = new Vector2(float.Parse(properties[1]), float.Parse(properties[2]));
                events.Add(new UnityEvent());
                events[events.Count - 1].AddListener(() => eventCommands.Move(sceneNo, pos));
                break;
            case 6://条件分岐
                properties = param1.Split('|');
                string[] p = new string[properties.Length];
                for (int i = 0; i < p.Length; i++)
                {
                    p[i] = string.Copy(properties[i]);
                }
                events.Add(new UnityEvent());
                events[events.Count - 1].AddListener(() => eventCommands.SetBoolean(p));
                events.Add(new UnityEvent());
                events[events.Count - 1].AddListener(() => JumpCommand());
                break;
            case 7://勧誘
                string[] ms = param1.Split('|');
                int no = int.Parse(ms[0]);
                Vector2 vpos = new Vector2(150, -400);
                branch = new string[3] { ms[1], ms[2], ms[3] };
                events.Add(new UnityEvent());
                events[events.Count - 1].AddListener(() => eventCommands.WriteStatusOne(no));
                events.Add(new UnityEvent());
                events[events.Count - 1].AddListener(() => eventCommands.MakeChoices(false, vpos, 700, -1, branch));
                events.Add(new UnityEvent());
                events[events.Count - 1].AddListener(() => eventCommands.WaitForChoosing());
                events.Add(new UnityEvent());
                events[events.Count - 1].AddListener(() => eventCommands.CloseMessage(true));
                events.Add(new UnityEvent());
                events[events.Count - 1].AddListener(() => eventCommands.CloseChoices());
                events.Add(new UnityEvent());
                events[events.Count - 1].AddListener(() => JumpCommand());
                break;
            case 8://パーティ追加
                int charaNo = int.Parse(param1);
                events.Add(new UnityEvent());
                events[events.Count - 1].AddListener(() => eventCommands.AddParty(charaNo));
                break;
            case 9://パーティ変更
                int changeNo = int.Parse(param1);
                string[] choices = new string[3] {PlayerData.Instance.party[1].name,
                PlayerData.Instance.party[2].name,"やめる"};
                Vector2 choicePos = new Vector2(150, -400);
                events.Add(new UnityEvent());
                events[events.Count - 1].AddListener(() => eventCommands.WriteMessage("パーティを入れ替えてください"));
                events.Add(new UnityEvent());
                events[events.Count - 1].AddListener(() =>
                eventCommands.MakeChoices(false, choicePos, 500, -1, choices));
                events.Add(new UnityEvent());
                events[events.Count - 1].AddListener(() => eventCommands.WaitForChoosing());
                events.Add(new UnityEvent());
                events[events.Count - 1].AddListener(() => eventCommands.ChangeParty(changeNo));
                events.Add(new UnityEvent());
                events[events.Count - 1].AddListener(() => eventCommands.WaitForInput());
                events.Add(new UnityEvent());
                events[events.Count - 1].AddListener(() => eventCommands.CloseMessage(true));
                events.Add(new UnityEvent());
                events[events.Count - 1].AddListener(() => eventCommands.CloseChoices(true));
                break;
            case 10://分岐終点
                events.Add(new UnityEvent());
                string s = "選択肢終点";
                events[events.Count - 1].AddListener(() => JumpCommand(s));
                break;
            case 11://セーブロード
                bool c = param1[0].Equals("セーブ");
                events.Add(new UnityEvent());
                events[events.Count - 1].AddListener(() => eventCommands.SaveLoadData(c));
                break;
            case 12://ゲーム終了
                events.Add(new UnityEvent());
                events[events.Count - 1].AddListener(() => Application.Quit());
                break;
            case 13://フラグ
                bool d, e;
                d = false;
                e = true;
                events.Add(new UnityEvent());
                events[events.Count - 1].AddListener(() => eventCommands.CloseMessage(d, e));
                break;
        }
        //EventCommands.isProcessing = true;
    }

    public void ReadScript2()
    {
        events = new List<UnityEvent>();
        if (line == eventData.PageData[0].CommandDataArray.Length)
        {
            line = 0;
            EventCommands.isProcessing = false;
            currentAction = null;
            return;
        }

        eventCommands.actNo = 0;// 処理中のコマンド内での実行中イベントの番号

        Expression.Map.MapEvent.EventCommandBase command = eventData.PageData[0].CommandDataArray[line];
        command.StackEventsTo(events, eventCommands);
        UI.Map.MapActionFactory factory = new UI.Map.MapActionFactory(eventCommands);
        currentAction = factory.CreateActionFrom(command);
    }

    // 特定のラベルに至るまでコマンドをスキップ
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

    public void SetEventData(Expression.Map.MapEvent.EventData eventData)
    {
        this.eventData = eventData;
    }
}
