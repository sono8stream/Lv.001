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
    const int INTERVAL = 150;
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
    public  int target;//かばわれているターゲットの番号、かばう実行時などに処理される、それ以外は-1
    public int kabauTarget;//かばっている対象
    bool isEnemy;//二回行動判定用
    bool win;//勝った？
    [SerializeField]
    AudioClip[] se;

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
        isEnemy = false;
        win = false;
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
                if (battlerNo >= battlerData.Count)
                {
                    phaseNo = (int)PHASE.End;
                }
                else if (interCount == 0)
                {
                    if (battlerData[battlerNo].status[(int)STATUS.HP] == 0)
                    {
                        interCount = 0;
                        battlerNo++;
                    }
                    else if (battlerData[battlerNo].isAlly)
                    {
                        switch (battlerData[battlerNo].jobNo)
                        {
                            case (int)JobType.就活生:
                                Attack();
                                break;
                            case (int)JobType.魔導士:
                                Magic();
                                break;
                            case (int)JobType.戦士:
                                Dash();
                                break;
                            case (int)JobType.武闘家:
                                Guard();
                                break;
                            case (int)JobType.医者:
                                Recover();
                                break;
                            case (int)JobType.学者:
                                Search();
                                break;
                            case (int)JobType.アル中:
                                DrunkAttack();
                                break;
                            case (int)JobType.踊り子:
                                Dance();
                                break;
                            case (int)JobType.勇者:
                                DragonKill();
                                break;
                        }
                    }
                    else
                    {
                        if (Random.Range(0, 5) < 3)
                        {
                            Attack();
                        }
                        else
                        {
                            Fire();
                        }
                        isEnemy = !isEnemy;
                    }
                }
                else if (interCount >= INTERVAL)
                {
                    if (!battlerData[battlerNo].isAlly)
                    {
                        effect.transform.localScale = Vector3.one;
                    }
                    interCount = 0;
                    if (End())
                    {
                        phaseNo = (int)PHASE.Last;
                    }
                    if (!isEnemy)
                    {
                        battlerNo++;
                    }
                }
                else
                {
                    interCount++;
                }
                break;
            case (int)PHASE.End:
                phaseNo = (int)PHASE.Standby;
                break;
            case (int)PHASE.Last:
                if (interCount == 0)
                {
                    if (win)
                    {
                        message.text = "ドラゴン討伐！！";
                    }
                    else
                    {
                        message.text = "全滅しました...";
                    }
                    interCount++;
                }
                else if(interCount>100)
                {
                    int no;
                    if(win)
                    {
                        no = 5;
                        PlayerData.Instance.pos = new Vector2(0, 2.5f);
                    }
                    else
                    {
                        no = 2;
                    }
                    UnityEngine.SceneManagement.SceneManager.LoadScene(no);
                }
                else
                {
                    interCount++;
                }
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
        if (allyCount == -1)
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

    bool Attack()//ダメージ計算,気合い切り
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
                int tarNo = 0;
                while (allyData[tarNo].status[(int)STATUS.HP] == 0)
                {
                    tarNo = Random.Range(0, allyCount);
                }
                diffencer = target == tarNo ? allyData[kabauTarget] : allyData[tarNo];
                effectScale = 0.5f;
            }
            damage= battlerData[battlerNo].status[(int)STATUS.ATT] - diffencer.status[(int)STATUS.DEF];
            damage += Random.Range(-30, 30);
            diffencer.status[(int)STATUS.HP] -= damage;
            effect.transform.localScale *= effectScale;
            effect.transform.position = diffencer.transform.position;
            effect.GetComponent<Animator>().SetTrigger("Slash");
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

    bool Dash()//ダメージ計算,体当たり
    {
        bool ret = false;
        if (battlerData[battlerNo].status[(int)STATUS.HP] == 0)
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
                int tarNo = 0;
                while (allyData[tarNo].status[(int)STATUS.HP] == 0)
                {
                    tarNo = Random.Range(0, allyCount);
                }
                diffencer = target == tarNo ? allyData[kabauTarget] : allyData[tarNo];
                effectScale = 0.5f;
            }
            damage = battlerData[battlerNo].status[(int)STATUS.ATT] * 3 / 2 - diffencer.status[(int)STATUS.DEF];
            diffencer.status[(int)STATUS.HP] -= damage;
            battlerData[battlerNo].status[(int)STATUS.HP] -= damage / 3;
            effect.transform.localScale *= effectScale;
            effect.transform.position = diffencer.transform.position;
            effect.GetComponent<Animator>().SetTrigger("Dash");
            diffencer.GetComponent<Animator>().SetTrigger("Damaged");
            battlerData[battlerNo].GetComponent<Animator>().SetTrigger("Damaged");
            message.text = battlerData[battlerNo].myName + "の体当たり！\r\n" + damage.ToString() + "のダメージ！　反動を受けた！";
            End();
            if (diffencer.isAlly)
            {
                diffencer.SetStatus();
            }
            else
            {
                diffencer.SetHPBar();
                battlerData[battlerNo].SetStatus();
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

    bool Recover()//ダメージ回復,治療
    {
        bool ret = false;
        if (battlerData[battlerNo].status[(int)STATUS.HP] == 0)
        {
            return true;
        }
        if (interCount == 0)
        {
            interCount++;
            StatusManager diffencer;
            float effectScale = 1;
            int recover;
                int tarNo = 0;
            for (int i = 0; i < allyData.Count; i++)
            {
                if (allyData[tarNo].status[(int)STATUS.HP] == 0)
                {
                    tarNo = i;
                }
                else
                {
                    if (allyData[i].status[(int)STATUS.HP] < allyData[tarNo].status[(int)STATUS.HP])
                    {
                        tarNo = i;
                    }
                }
            }
            diffencer = allyData[tarNo];
                effectScale = 0.5f;
            recover = battlerData[battlerNo].status[(int)STATUS.MHP] / 2;
            diffencer.status[(int)STATUS.HP] += recover;
            if(diffencer.status[(int)STATUS.HP]>diffencer.status[(int)STATUS.MHP])
            {
                diffencer.status[(int)STATUS.HP] = diffencer.status[(int)STATUS.MHP];
            }
            effect.transform.localScale *= effectScale;
            effect.transform.position = diffencer.transform.position;
            effect.GetComponent<Animator>().SetTrigger("Recover");
            diffencer.GetComponent<Animator>().SetTrigger("Recovered");
            message.text = battlerData[battlerNo].myName + "は治療した！\r\n"
                + diffencer.myName + "のHPが" + recover.ToString() + "回復！";
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
        }
        else
        {
            interCount++;
        }
        return ret;
    }

    bool Guard()//HP最小の仲間をかばう
    {
        bool ret = false;
        if (battlerData[battlerNo].status[(int)STATUS.HP] == 0)
        {
            return true;
        }
        if (interCount == 0)
        {
            interCount++;
            StatusManager diffencer;
            int tarNo = 0;
            for (int i = 0; i < allyData.Count; i++)
            {
                if (allyData[tarNo].status[(int)STATUS.HP] == 0)
                {
                    tarNo = i;
                }
                else
                {
                    if (allyData[i].status[(int)STATUS.HP] < allyData[tarNo].status[(int)STATUS.HP])
                    {
                        tarNo = i;
                    }
                }
            }
            if (allyData[tarNo] == battlerData[battlerNo])
            {
                Attack();
            }
            else
            {
                target = tarNo;
                kabauTarget = allyData.IndexOf(battlerData[battlerNo]);
                battlerData[battlerNo].GetComponent<Animator>().SetTrigger("Recovered");
                message.text = battlerData[battlerNo].myName + "は"+ allyData[target].myName
                    + "をかばっている！";
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
        }
        else
        {
            interCount++;
        }
        return ret;
    }

    bool Dance()//ダンス　死者蘇生
    {
        bool ret = false;
        if (battlerData[battlerNo].status[(int)STATUS.HP] == 0)
        {
            return true;
        }
        if (interCount == 0)
        {
            interCount++;
            StatusManager diffencer;
            float effectScale = 1;
            int recover;
            int tarNo = -1;
            for (int i = 0; i < allyCount; i++)
            {
                if (allyData[i].status[(int)STATUS.HP] == 0)
                {
                    tarNo = i;
                }
            }
            if (tarNo==-1)
            {
                Attack();
            }
            else
            {
                allyData[tarNo].status[(int)STATUS.HP] = allyData[tarNo].status[(int)STATUS.MHP] / 2;
                allyData[tarNo].GetComponent<Animator>().SetTrigger("Recovered");
                effect.transform.localScale *= effectScale;
                effect.transform.position = allyData[tarNo].transform.position;
                effect.GetComponent<Animator>().SetTrigger("Recover");
                message.text = battlerData[battlerNo].myName + "の復活の舞！" + allyData[tarNo].myName
                    + "は復活した！";
                if (allyData[tarNo].isAlly)
                {
                    allyData[tarNo].SetStatus();
                }
                else
                {
                    allyData[tarNo].SetHPBar();
                }
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
        }
        else
        {
            interCount++;
        }
        return ret;
    }

    bool Magic()//呪文
    {
        bool ret = false;
        if (battlerData[battlerNo].status[(int)STATUS.HP] == 0)
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
                int tarNo = 0;
                while (allyData[tarNo].status[(int)STATUS.HP]== 0)
                {
                    tarNo = Random.Range(0, allyCount);
                }
                diffencer = target == tarNo ? allyData[kabauTarget] : allyData[tarNo];
                effectScale = 0.5f;
            }
            effectScale = 0.5f;
            damage = (int)(battlerData[battlerNo].status[(int)STATUS.ATT] * Random.Range(0.7f, 2));//振れ幅大
            diffencer.status[(int)STATUS.HP] -= damage;
            effect.transform.localScale *= effectScale;
            effect.transform.position = diffencer.transform.position;
            effect.GetComponent<Animator>().SetTrigger("Magic");
            diffencer.GetComponent<Animator>().SetTrigger("Damaged");
            message.text = battlerData[battlerNo].myName + "は呪文を唱えた！"
                + damage.ToString() + "のダメージ！";
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

    bool DrunkAttack()//千鳥足
    {
        bool ret = false;
        if (battlerData[battlerNo].status[(int)STATUS.HP] == 0)
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
                int tarNo = 0;
                while (allyData[tarNo].status[(int)STATUS.HP] == 0)
                {
                    tarNo = Random.Range(0, allyCount);
                }
                diffencer = target == tarNo ? allyData[kabauTarget] : allyData[tarNo];
                effectScale = 0.5f;
            }
            effectScale = 0.5f;
            damage = Random.Range(0, 4) == 3 ? 4 : 0;
            damage *= battlerData[battlerNo].status[(int)STATUS.ATT];//振れ幅大
            diffencer.status[(int)STATUS.HP] -= damage;
            effect.transform.localScale *= effectScale;
            effect.transform.position = diffencer.transform.position;
            if (damage == 0)
            {
                effect.GetComponent<Animator>().SetTrigger("Miss");
            }
            else
            {
                effect.GetComponent<Animator>().SetTrigger("DrunkAttack");
            }
            diffencer.GetComponent<Animator>().SetTrigger("Damaged");
            message.text = battlerData[battlerNo].myName + "、惑いの千鳥足！" + damage.ToString() + "のダメージ！";
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

    bool Search()//調査、仲間の能力をランダムで強化、敵弱化
    {
        bool ret = false;
        if (battlerData[battlerNo].status[(int)STATUS.HP] == 0)
        {
            return true;
        }
        if (interCount == 0)
        {
            interCount++;
            StatusManager diffencer;
            float effectScale = 1;
            int damage;
            int tarNo = 0;
            while (tarNo == allyCount || allyData[tarNo].status[(int)STATUS.HP] == 0)
            {
                tarNo = Random.Range(0, allyCount + 1);
            }
            if(tarNo<allyCount)
            {
                diffencer = allyData[tarNo];
                damage = diffencer.status[(int)STATUS.ATT] / 4;
                diffencer.status[(int)STATUS.ATT] += damage;
                message.text = battlerData[battlerNo].myName 
                    + "は研究・検証を行った！\r\n" + damage.ToString() + "の攻撃力が上がった！";
                diffencer.GetComponent<Animator>().SetTrigger("Recovered");
            }
            else
            {
                diffencer = enemyStatus;
                damage = diffencer.status[(int)STATUS.ATT] / 8;
                diffencer.status[(int)STATUS.ATT] -= damage;
                message.text = battlerData[battlerNo].myName
                    + "は研究・検証を行った！\r\n" + damage.ToString() + "の攻撃力が下がった！";
                diffencer.GetComponent<Animator>().SetTrigger("Damaged");
            }
            effectScale = 0.5f;
            effect.transform.localScale *= effectScale;
            effect.transform.position = diffencer.transform.position;
            effect.GetComponent<Animator>().SetTrigger("ParamChange");
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

    bool DragonKill()//読んで字のごとく　勇者専用
    {
        bool ret = false;
        if (battlerData[battlerNo].status[(int)STATUS.HP] == 0)
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
                int tarNo = 0;
                while (allyData[tarNo].status[(int)STATUS.HP] == 0)
                {
                    tarNo = Random.Range(0, allyCount);
                }
                diffencer = target == tarNo ? allyData[kabauTarget] : allyData[tarNo];
                effectScale = 0.5f;
            }
            effectScale = 0.5f;
            damage = battlerData[battlerNo].status[(int)STATUS.ATT]*2;//二倍ダメージ！
            diffencer.status[(int)STATUS.HP] -= damage;
            effect.transform.localScale *= effectScale;
            effect.transform.position = diffencer.transform.position;
            effect.GetComponent<Animator>().SetTrigger("LargeSlash");
            diffencer.GetComponent<Animator>().SetTrigger("Damaged");
            message.text = battlerData[battlerNo].myName + "のドラゴンキリング！\r\n" 
                + damage.ToString() + "のダメージ！";
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

    bool Fire()//ダメージ計算,気合い切り
    {
        bool ret = false;
        if (battlerData[battlerNo].status[(int)STATUS.HP] == 0)
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
                effect.transform.position = diffencer.transform.position;
                damage = battlerData[battlerNo].status[(int)STATUS.ATT] / 2 - diffencer.status[(int)STATUS.DEF];
                damage += Random.Range(-30, 30);
                diffencer.status[(int)STATUS.HP] -= damage;
                diffencer.GetComponent<Animator>().SetTrigger("Damaged");
                End();
                diffencer.SetHPBar();
            }
            else
            {
                int tarNo = 0;
                for (int i = 0; i < allyCount; i++)
                {
                    if (allyData[i].status[(int)STATUS.HP] > 0)
                    {
                        diffencer = target == tarNo ? allyData[kabauTarget] : allyData[i];
                        damage = battlerData[battlerNo].status[(int)STATUS.ATT] / 2 - diffencer.status[(int)STATUS.DEF];
                        damage += Random.Range(-30, 30);
                        diffencer.status[(int)STATUS.HP] -= damage;
                        diffencer.GetComponent<Animator>().SetTrigger("Damaged");
                        End();
                        diffencer.SetStatus();
                    }
                }
                effect.transform.localPosition = new Vector2(0, allyData[0].transform.position.y - 100);
            }
            effect.transform.localScale *= effectScale;
            effect.GetComponent<Animator>().SetTrigger("Fire");
            message.text = battlerData[battlerNo].myName + "は火を吹いた！\r\n" + "全体にダメージ！";
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
            win = true;
            return ending;
        }
        ending = true;
        for (int i = 0; i < allyCount; i++)
        {
            if (allyData[i].status[(int)STATUS.HP] <= 0)
            {
                ending &= true;
                allyData[i].status[(int)STATUS.HP] = 0;
                if (target != -1 && kabauTarget == i)
                {
                    target = -1;
                    kabauTarget = -1;
                }
            }
            else
            {
                ending &= false;
            }

        }
        return ending;
    }

    void SetSE(int seNo)
    {
        AudioSource sound = GetComponent<AudioSource>();
        sound.clip = se[seNo];
        sound.Play();
    }
}
