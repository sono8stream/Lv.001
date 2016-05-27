using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class BattleController : MonoBehaviour {

    enum PHASE
    {
        Start = 0, Standby, Battle, End,Last
    }
    int phaseNo;
    const int INTERVAL = 30;
    int interCount;
    [SerializeField]
    Text message;
    [SerializeField]
    GameObject effect;
    [SerializeField]
    StatusManager enemyStatus;
    [SerializeField]
    StatusManager[] allyStatus;
    List<StatusManager> allyData;//戦闘非参加者を除外
    List<StatusManager> battlerData;//戦闘参加者（敵味方両方）の行動順に合わせたリスト
    int battlerNo;
    int allyCount;
    int target;//ターゲットの番号、かばう実行時などに処理される、それ以外は-1

    // Use this for initialization
    void Start()
    {
        phaseNo = (int)PHASE.Start;
        interCount = 0;
        battlerNo = 0;
        allyCount = -1;
        allyData = new List<StatusManager>();
        for(int i=0;i<allyStatus.Length;i++)
        {
            if(allyStatus[i].exist)
            {
                allyData.Add(allyStatus[i]); 
            }
        }
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
            case (int)PHASE.Last:
                message.text = "全滅しました...";
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
            allyCount = allyData.Count;
        }
        battlerData.Add(enemyStatus);
        int btlCount = battlerData.Count;
        for (int i = 0; i < allyCount; i++)
        {
            for (int j = 0; j < btlCount; j++)
            {
                if (allyData[i].status[(int)STATUS.SPD] > battlerData[j].status[(int)STATUS.SPD])
                {
                    battlerData.Insert(j, allyData[i]);
                    break;
                }
                else if (j == btlCount - 1)
                {
                    battlerData.Add(allyData[i]);
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
        if(battlerData[battlerNo].status[(int)STATUS.HP]==0)
        {
            return true;
        }
        if (interCount == 0)
        {
            interCount++;
            StatusManager diffencer;
            float effectScale = 1;
            int damage;
            if (battlerData[battlerNo].isAlly)
            {
                diffencer = enemyStatus;
            }
            else
            {
                diffencer = allyData[Random.Range(0, allyCount)];
                effectScale = 0.5f;
            }
            damage= battlerData[battlerNo].status[(int)STATUS.ATT] - diffencer.status[(int)STATUS.DEF];
            diffencer.status[(int)STATUS.HP] -= damage;
            effect.transform.localScale *= effectScale;
            effect.transform.position = diffencer.transform.position;
            effect.GetComponent<Animator>().SetTrigger(battlerData[battlerNo].attackTrigger);
            diffencer.GetComponent<Animator>().SetTrigger("Damaged");
            message.text = battlerData[battlerNo].myName + "の攻撃！\r\n"+damage.ToString()+"のダメージ！";
            End();
            if (diffencer.isAlly)
            {
                diffencer.SetStatus();
            }
            else
            {
                diffencer.SetHPBar();
            }
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
            if (End())
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(1);
            }
        }
        else
        {
            interCount++;
        }
        return ret;
    }

    bool End()
    {
        bool ending = false;
        ending = enemyStatus.status[(int)STATUS.HP] <= 0;
        if(ending)
        {
            enemyStatus.status[(int)STATUS.HP] = 0;
            return ending;
        }
        ending = true;
        for (int i = 0; i < allyCount; i++)
        {
            if (allyData[i].status[(int)STATUS.HP] <= 0)
            {
                ending &= true;
                allyData[i].status[(int)STATUS.HP] = 0;
            }
            else
            {
                ending &= false;
            }

        }
        return ending;
    }
}
