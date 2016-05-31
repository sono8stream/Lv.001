using UnityEngine;
using System;
using System.Collections;
using UnityEngine.Events;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
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

[Serializable]
public class PlayerData
{
    [NonSerialized]
    public List<Sprite[]> spritesM;
    [NonSerialized]
    public List<Sprite[]> spritesF;
    private static PlayerData instance;
    public static PlayerData Instance
    {
        get
        {
            if (instance == null)
            {
                int sceneCount = /*UnityEngine.SceneManagement.SceneManager.sceneCount*/6;
                instance = new PlayerData();
                instance.money = 1500;
                instance.party = new List<Unit>();
                instance.party.Add(new Unit("あなた", 1, 100, 10, 10, 10, 0,true, null));
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
                    int jobPat = UnityEngine.Random.Range(0, jobs.Count);
                    int jobNo = jobs[jobPat];
                    jobs.RemoveAt(jobPat);
                    //bool male = Random.Range(0, 2) == 1 && maleCount == 0;
                    bool male = i < maleCount;
                    string name;
                    int lvPat = UnityEngine.Random.Range(0, lvs.Count);
                    int lv = UnityEngine.Random.Range(lvs[lvPat], lvs[lvPat] +99);
                    lvs.RemoveAt(lvPat);
                    if (male)
                    {
                        //maleCount--;
                        int namePat = UnityEngine.Random.Range(0, namesM.Count);
                        name = namesM[namePat];
                        namesM.RemoveAt(namePat);
                        s = instance.spritesM[jobNo - 1];
                    }
                    else
                    {
                        int namePat = UnityEngine.Random.Range(0, namesF.Count);
                        name = namesF[UnityEngine.Random.Range(0, namesF.Count)];
                        namesF.RemoveAt(namePat);
                        s = instance.spritesF[jobNo - 1];
                    }
                    instance.characters.Add(new Unit(name, lv,
                        UnityEngine.Random.Range(100, 300) + lv / 2, lv, lv, lv, jobNo, male, s));

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
    [NonSerialized]
    public Vector2 pos;//移動時などに初期化する際、使用

    public List<int[]>[] selfVars;//各イベントが持つセルフ変数のリスト
    //セーブ用
    private static readonly string savePath = Application.dataPath + "/save.bytes";

    private PlayerData() { }

    public void SaveSelfVars(int sceneNo)
    {
        instance.selfVars[sceneNo] = new List<int[]>();
        GameObject[] g = GameObject.FindGameObjectsWithTag("Event");
        if (g != null)
        {
            for (int i = 0; i < g.Length; i++)
            {
                instance.selfVars[sceneNo].Add(g[i].GetComponent<EventCommands>().SelfVar);
            }
        }
    }

    public void LoadSelfVars(int sceneNo)
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

    public void SaveData()
    {
#if UNITY_IPHONE || UNITY_IOS
		System.Environment.SetEnvironmentVariable("MONO_REFLECTION_SERIALIZER", "yes");
#endif
        using (FileStream fs = new FileStream(savePath, FileMode.Create, FileAccess.Write))
        {
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(fs, PlayerData.instance);
        }
        Debug.Log("save");
    }

    public void LoadData()
    {
#if UNITY_IPHONE || UNITY_IOS
		System.Environment.SetEnvironmentVariable("MONO_REFLECTION_SERIALIZER", "yes");
#endif
        //instance = null;
        using (FileStream fs = new FileStream(savePath, FileMode.Open, FileAccess.Read))
        {
            BinaryFormatter bf = new BinaryFormatter();
            instance = bf.Deserialize(fs) as PlayerData;
        }
        Debug.Log("ok?");
        instance.pos = Vector2.up * 0.5f;
        SetSprites();
        Debug.Log("load");
    }

    public void SetSprites()
    {
        const int charaCount = 7;
        instance.spritesM = new List<Sprite[]>();
        instance.spritesF = new List<Sprite[]>();
        for (int i = 0; i < charaCount; i++)
        {
            string path = "Sprites/Charas/" + (i + 1).ToString();
            Sprite[] spriteMAll = Resources.LoadAll<Sprite>(path + "m");
            instance.spritesM.Add(new Sprite[3] { spriteMAll[0], spriteMAll[1], spriteMAll[2] });
            Sprite[] spriteFAll = Resources.LoadAll<Sprite>(path + "f");
            instance.spritesF.Add(new Sprite[3] { spriteFAll[0], spriteFAll[1], spriteFAll[2] });
        }
        for (int i = 0; i < charaCount; i++)
        {
            if (characters[i].status[(int)StatusParams.skillNo] > 0)
            {
                if (instance.characters[i].isMale)
                {
                    instance.characters[i].sprite
                        = instance.spritesM[instance.characters[i].status[(int)StatusParams.skillNo] - 1];
                }
                else
                {
                    instance.characters[i].sprite
                        = instance.spritesF[instance.characters[i].status[(int)StatusParams.skillNo] - 1];
                }
            }
        }
    }

    public bool Save()
    {
        MemoryStream memoryStream = new MemoryStream();
#if UNITY_IPHONE || UNITY_IOS
		System.Environment.SetEnvironmentVariable("MONO_REFLECTION_SERIALIZER", "yes");
#endif
        BinaryFormatter bf = new BinaryFormatter();
        bf.Serialize(memoryStream, instance);

        string tmp = System.Convert.ToBase64String(memoryStream.ToArray());
        try
        {
            PlayerPrefs.SetString(savePath, tmp);
        }
        catch (PlayerPrefsException)
        {
            return false;
        }
        PlayerPrefs.Save();
        return true;
    }

    public PlayerData Load()
    {
        if (!PlayerPrefs.HasKey(savePath)) return default(PlayerData);
#if UNITY_IPHONE || UNITY_IOS
		System.Environment.SetEnvironmentVariable("MONO_REFLECTION_SERIALIZER", "yes");
#endif
        BinaryFormatter bf = new BinaryFormatter();
        string serializedData = PlayerPrefs.GetString(savePath);

        MemoryStream dataStream = new MemoryStream(System.Convert.FromBase64String(serializedData));
        instance = (PlayerData)bf.Deserialize(dataStream);

        instance.pos = Vector2.up * 0.5f;
        SetSprites();
        return instance;
    }
}

[Serializable]
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

[Serializable]
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
    public bool isMale;
    [NonSerialized]
    public Sprite[] sprite;
    public bool onceFriend;
    public Unit(string name,int Lv, int HP, int attack, int defence, int speed, int skillNo,bool isMale,Sprite[] sprite)
    {
        this.name = name;
        status = new int[7] {Lv, HP, HP, attack, defence, speed, skillNo };
        this.isMale = isMale;
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
    気合い切り = 0, 呪文, 体当たり, かばう, 治療, 千鳥足, 検証, 復活の舞, ドラゴンキル
}

public enum StatusParams
{
    Lv = 0, MaxHp, HP, attack, defence, speed, skillNo
}
