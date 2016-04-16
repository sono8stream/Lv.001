using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using System.Collections.Generic;

public static class Data
{
    public static List<Item> items;//アイテムの所持数
    static Data()
    {
        items = new List<Item>();
        items.Add(new Item("勇者の聖杯", (int)ItemType.防具, 990000, 100));
        items.Add(new Item("鉄の剣", (int)ItemType.武器, 400, 50));
        items.Add(new Item("銀の剣", (int)ItemType.武器, 1000, 200));
        items.Add(new Item("傷薬", (int)ItemType.道具, 100, 100));
        items.Add(new Item("なまら傷薬", (int)ItemType.道具, 1000, 1000));
    }
}

public static class PlayerData
{
    public static int money;
    static PlayerData()
    {
        money = 1500;
    }
}

public class Item
{
    public string name;
    int type;
    public int possessionCount;//所持数
    public int price;
    int param;
    UnityEvent useEffect;//使用時効果
    public Item(string name ,int type,int price,int param)
    {
        this.name = name;
        this.type = type;
        this.price = price;
        this.param = param;
        possessionCount = 0;
        useEffect = null;
    }
}

public enum ItemType
{
    道具 = 0, 武器, 防具
}
