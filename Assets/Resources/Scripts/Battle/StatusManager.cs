using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;

public class StatusManager : MonoBehaviour {

    public int[] status;
    public bool isAlly;//味方か
    public bool exist;
    public string attackTrigger;//攻撃時のエフェクトトリガー
    public int charaNo;
    public string myName;
    public int jobNo;
    public Text charaName, hp, lv;
    public RectTransform gage;

    void Awake()
    {
        if (isAlly)
        {
            if (PlayerData.Instance.party.Count <= charaNo)
            {
                exist = false;
                gameObject.SetActive(false);
                return;
            }
            exist = true;
            myName = PlayerData.Instance.party[charaNo].name;
            int pow = PlayerData.Instance.party[charaNo].weapon.name.Equals("--") ? 0 :
                PlayerData.Instance.party[charaNo].weapon.param;
            if (PlayerData.Instance.party[charaNo].weapon.name.Equals("勇者の聖杯"))
            {
                PlayerData.Instance.party[charaNo].status[(int)StatusParams.skillNo] = (int)JobType.勇者;
            }
            status = new int[Enum.GetNames(typeof(STATUS)).Length];
            status[(int)STATUS.ATT] = PlayerData.Instance.party[charaNo].status[(int)StatusParams.Lv]+pow;
            status[(int)STATUS.DEF] = 0;
            status[(int)STATUS.HP] = PlayerData.Instance.party[charaNo].status[(int)StatusParams.HP]+pow/2;
            status[(int)STATUS.MHP] = status[(int)STATUS.HP];
            status[(int)STATUS.SPD] = PlayerData.Instance.party[charaNo].status[(int)StatusParams.Lv]+pow;
            charaName = transform.Find("Name").GetComponent<Text>();
            charaName.text = myName;
            hp = transform.Find("Name (2)").GetComponent<Text>();
            lv = transform.Find("Name (4)").GetComponent<Text>();
            lv.text = PlayerData.Instance.party[charaNo].status[(int)StatusParams.Lv].ToString();
            jobNo = PlayerData.Instance.party[charaNo].status[(int)StatusParams.skillNo];
            SetStatus();
        }
    }

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetStatus()
    {
        hp.text = status[(int)STATUS.HP].ToString();
    }

    public void SetHPBar()
    {
        gage.localScale = new Vector3(status[(int)STATUS.HP] / (float)status[(int)STATUS.MHP], 1, 1);
    }
}

public enum STATUS
{
    MHP = 0, HP, ATT, DEF, SPD
}
