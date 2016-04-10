using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class StatusManager : MonoBehaviour {

    public int[] status;
    public bool isAlly;//味方か
    public string attackTrigger;//攻撃時のエフェクトトリガー

    void Awake()
    {
        status = new int[Enum.GetNames(typeof(STATUS)).Length];
        for (int i = 0; i < status.GetLength(0); i++)
        {
            status[i] = 100;
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
}

public enum STATUS
{
    MHP = 0, HP, ATT, DEF, SPD
}
