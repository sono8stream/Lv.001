using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using System.Collections.Generic;
using System.IO;
using System.Text;
using LitJson;

public class Data
{
    private static Data instance;
    public static Data Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new Data();
                instance.items = new List<Item>();
                instance.items.Add(new Item("勇者の聖杯", (int)ItemType.武器, 990000, 499, "勇者の証を得る"));
                instance.items.Add(new Item("鉄の剣", (int)ItemType.武器, 400, 50, "鉄でできた剣 Lv+49"));
                instance.items.Add(new Item("銀の剣", (int)ItemType.武器, 1000, 200, "銀でできた剣 Lv+199"));
                instance.items.Add(new Item("傷薬", (int)ItemType.道具, 100, 100, "傷を癒す薬 回復+100"));
                instance.items.Add(new Item("なまら傷薬", (int)ItemType.道具, 1000, 1000, "深い傷を癒す薬 回復+1000"));
                instance.items.Add(new Item("--", (int)ItemType.武器, 0, 0, "何も装備していない。"));
            }
            return instance;
        }
        set
        {
            instance = value;
        }
    }
    public List<Item> items;//アイテムの所持数
    private Data() { }
}

public class PlayerData
{
    public List<Sprite[]> spritesM;
    public List<Sprite[]> spritesF;
    private static PlayerData instance;
    public static PlayerData Instance
    {
        get
        {
            if (instance == null)
            {
                int sceneCount = 5;
                instance = new PlayerData();
                instance.money = 1500;
                instance.party = new List<Unit>();
                instance.party.Add(new Unit("主人公", 1, 100, 10, 10, 10, 0, null));
                const int charaCount = 7;
                instance.spritesM = new List<Sprite[]>();
                instance.spritesF = new List<Sprite[]>();
                for (int i = 0; i < charaCount; i++)
                {
                    string path = "Sprites/Charas/" + (i+1).ToString();
                    Sprite[] spriteMAll = Resources.LoadAll<Sprite>(path + "m");
                    instance.spritesM.Add(new Sprite[3] { spriteMAll[0], spriteMAll[1], spriteMAll[2] });
                    Sprite[] spriteFAll = Resources.LoadAll<Sprite>(path + "f");
                    instance.spritesF.Add(new Sprite[3] { spriteFAll[0], spriteFAll[1], spriteFAll[2] });
                }
                int maleCount = 4;
                Sprite[] s = null;
                List<int> lvs = new List<int>(charaCount) { 1, 100, 200, 300, 400, 600, 800 };
                List<int> jobs = new List<int>(charaCount) { 1, 2, 3, 4, 5, 6, 7 };
                List<string> namesM = new List<string>(4) { "アレン","バート","トム","ジョン" };
                List<string> namesF = new List<string>(4) { "クレア", "ドリス", "ローラ", "モリー" };
                instance.characters = new List<Unit>();
                for (int i = 0; i < charaCount; i++)
                {
                    int jobPat = Random.Range(0, jobs.Count);
                    int jobNo = jobs[jobPat];
                    jobs.RemoveAt(jobPat);
                    //bool male = Random.Range(0, 2) == 1 && maleCount == 0;
                    bool male = i < maleCount;
                    string name;
                    int lvPat = Random.Range(0, lvs.Count);
                    int lv = Random.Range(lvs[lvPat], lvs[lvPat] +99);
                    lvs.RemoveAt(lvPat);
                    if (male)
                    {
                        //maleCount--;
                        int namePat = Random.Range(0, namesM.Count);
                        name = namesM[namePat];
                        namesM.RemoveAt(namePat);
                        s = instance.spritesM[jobNo-1];
                    }
                    else
                    {
                        int namePat = Random.Range(0, namesF.Count);
                        name = namesF[Random.Range(0, namesF.Count)];
                        namesF.RemoveAt(namePat);
                        s = instance.spritesF[jobNo-1];
                    }
                    instance.characters.Add(new Unit(name, lv,
                        Random.Range(100, 500) + lv / 2, lv, lv, lv, jobNo,s));

                }
                instance.pos = Vector2.up * 0.5f;
                //instance.selfVars = new List<int[]>[UnityEngine.SceneManagement.SceneManager.sceneCount];
                instance.selfVars = new List<int[]>[sceneCount];
                /*instance.party.Add(instance.characters[0]);
                instance.party.Add(instance.characters[1]);*/
                //instance.party.Add(instance.characters[2]);
            }
            return instance;

        }
        set
        {
            instance = value;
        }
    }
    public int money;
    public List<Unit> party;
    public List<Unit> characters;
    public Vector2 pos;//移動時などに初期化する際、使用

    public List<int[]>[] selfVars;//各イベントが持つセルフ変数のリスト
    //セーブ用
    private static readonly string savePath = Application.persistentDataPath;

    private PlayerData() { }

    static void Save()
    {
        instance.pos = Vector2.up * 0.5f;
        string json = JsonMapper.ToJson(instance);
        File.WriteAllText(savePath + "/SaveData.json", json);
    }

    static void Load()
    {
        string filePath = savePath + "/SaveData.json";
        if (!File.Exists(filePath))
        {
            return;
        }
        string jsonText = File.ReadAllText(filePath);
        if (string.IsNullOrEmpty(jsonText))
        {
            return;
        }

        /// Json文字列をCharacterDataの配列に変換する
        instance = JsonMapper.ToObject<PlayerData>(jsonText);
    }

    public static void SaveSelfVars(int sceneNo)
    {
        instance.selfVars[sceneNo] = new List<int[]>();
        GameObject[] g = GameObject.FindGameObjectsWithTag("Event");
        for (int i=0;i<g.Length;i++)
        {
            instance.selfVars[sceneNo].Add(g[i].GetComponent<EventCommands>().SelfVar);
        }
    }

    public static void LoadSelfVars(int sceneNo)
    {
        /*if(instance.selfVars.Length==0||sceneNo>=instance.selfVars.Length
            ||(sceneNo<instance.selfVars.Length&&instance.selfVars[sceneNo]==null))
        {
            return;
        }*/
        try
        {
            GameObject[] g = GameObject.FindGameObjectsWithTag("Event");
            if (g == null)
            {
                return;
            }
            for (int i = 0; i < g.Length; i++)
            {
                g[i].GetComponent<EventCommands>().SelfVar = instance.selfVars[sceneNo][i];
            }
        }
        catch { }
    }
}

public class Item
{
    public string name;
    public int type;
    public int possessionCount;//所持数
    public int price;
    public string exp;//アイテムの説明
    public int param;//lv上昇値
    UnityEvent useEffect;//使用時効果
    public Item(string name ,int type,int price,int param,string exp)
    {
        this.name = name;
        this.type = type;
        this.price = price;
        this.param = param;
        this.exp = exp;
        possessionCount = 0;
        useEffect = null;
    }
}

public enum ItemType
{
    道具 = 0, 武器, 防具
}

public class Unit
{
    public string name;
    public int Lv;
    public int MaxHP;
    public int HP;
    public int attack;
    public int defence;
    public int speed;
    public int skillNo;
    public int[] status;
    public Item weapon;
    public Sprite[] sprite;
    public bool onceFriend;
    public Unit(string name,int Lv, int HP, int attack, int defence, int speed, int skillNo,Sprite[] sprite)
    {
        this.name = name;
        status = new int[7] {Lv, HP, HP, attack, defence, speed, skillNo };
        this.sprite = sprite;
        weapon = Data.Instance.items[5];
        onceFriend = false;
    }
}

public enum JobType
{
    就活生 = 0, 魔導士, 戦士, 武闘家, 医者, アル中, 学者, 踊り子, 勇者
}

public enum SkillType
{
    気合い切り = 0, 呪文, 体当たり, かばう, 治療, 千鳥足, 調査, 勝利の舞, ドラゴンキル
}

public enum StatusParams
{
    Lv = 0, MaxHp, HP, attack, defence, speed, skillNo
}
