using System.Collections.Generic;
using UnityEngine;

public enum ItemType // 장착템 / 소모템 구분
{
    Equipment,
    Consumable
}

public enum ModifireType // 스탯 적용 방식. 고정형 / 배율형
{
    Flat,
    Mul
}

public class ItemData
{
    public string itemID;
    public ItemType itemType;
    public string name;

    public bool isStackable;

    // 장착템
    public ModifireType modifireType;
    public EquipmentStats equipmentStats;
    public string passiveID;

    // 소모템
    public TargetType target;
    public List<ItemEffect> itemEffect;
    public int maxStack;
    public int cooltime;
}

public class EquipmentStats
{
    public int flatHP;
    public int flatMP;
    public int flatSpeed;
    public float mulAttack;
    public float mulDefense;
    public float mulCritical;
}

public class ItemEffect
{
    public string effectID;
    public int value;
}
