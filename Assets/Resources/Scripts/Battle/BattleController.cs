using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class BattleController : MonoBehaviour {

    enum PHASE
    {
        Start = 0, Standby, Battle, End
    }
    int phaseNo;
    const int INTERVAL = 30;
    int interCount;
    [SerializeField]
    GameObject effect;
    [SerializeField]
    StatusManager enemyStatus;
    [SerializeField]
    StatusManager[] allyStatus;
    List<StatusManager> battlerData;//戦闘参加者（敵味方両方）の行動順に合わせたリスト
    int battlerNo;
    int allyCount;

    // Use this for initialization
    void Start()
    {
        phaseNo = (int)PHASE.Start;
        interCount = 0;
        battlerNo = 0;
        allyCount = -1;
    }

    // Update is called once per frame
    void Update()
    {
        switch(phaseNo)
        {
            case (int)PHASE.Start:
                phaseNo = (int)PHASE.Standby;
                break;
            case (int)PHASE.Standby:
                SetOrder();
                battlerNo = 0;
                phaseNo = (int)PHASE.Battle;
                break;
            case (int)PHASE.Battle:
                Attack();
                if(battlerNo>=battlerData.Count)
                {
                    phaseNo = (int)PHASE.End;
                }
                break;
            case (int)PHASE.End:
                phaseNo = (int)PHASE.Standby;
                break;
        }
        /*interCount++;
        if (interCount >= INTERVAL)
        {
            interCount = 0;
            phaseNo = (int)PHASE.End;
            if (End())
            {
                Application.Quit();
            }
            else
            {
                phaseNo = (int)PHASE.Battle;
            }
        }*/
    }

    void SetOrder()//戦闘者を速さ順に並べ替え
    {
        battlerData = new List<StatusManager>();
        if(allyCount==-1)
        {
            allyCount = allyStatus.GetLength(0);
        }
        battlerData.Add(enemyStatus);
        int btlCount = battlerData.Count;
        for (int i = 0; i < allyCount; i++)
        {
            for (int j = 0; j < btlCount; j++)
            {
                if (allyStatus[i].status[(int)STATUS.SPD] > battlerData[j].status[(int)STATUS.SPD])
                {
                    battlerData.Insert(j, allyStatus[i]);
                    break;
                }
                else if (j == btlCount - 1)
                {
                    battlerData.Add(allyStatus[i]);
                }
            }
            btlCount++;
        }
        allyCount = battlerData.Count(n => n.isAlly);
        Debug.Log(battlerData.Count);
    }

    bool Attack()//ダメージ計算
    {
        bool ret = false;
        if (interCount == 0)
        {
            interCount++;
            StatusManager diffencer;
            float effectScale = 1;
            if (battlerData[battlerNo].isAlly)
            {
                diffencer = enemyStatus;
            }
            else
            {
                diffencer = allyStatus[Random.Range(0, allyCount)];
                effectScale = 0.5f;
            }
            diffencer.status[(int)STATUS.HP] -= battlerData[battlerNo].status[(int)STATUS.ATT] - diffencer.status[(int)STATUS.DEF];
            effect.transform.localScale *= effectScale;
            effect.transform.position = diffencer.transform.position;
            effect.GetComponent<Animator>().SetTrigger(battlerData[battlerNo].attackTrigger);
            diffencer.GetComponent<Animator>().SetTrigger("Damaged");
            End();
        }
        else if (interCount >= INTERVAL)
        {
            if (!battlerData[battlerNo].isAlly)
            {
                effect.transform.localScale = Vector3.one;
            }
            interCount = -1;
            battlerNo++;
            ret = true;
        }
        interCount++;
        return ret;
    }

    bool End()
    {
        bool ending = false;
        ending = enemyStatus.status[(int)STATUS.HP] <= 0;
        if(ending)
        {
            return ending;
        }
        ending = true;
        for (int i = 0; i < allyCount; i++)
        {
            ending &= allyStatus[i].status[(int)STATUS.HP] <= 0;
        }
        return ending;
    }
}
