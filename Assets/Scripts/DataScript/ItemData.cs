using System;
using System.Collections.Generic;

public enum ItemType // 장착템 / 소모템 구분
{
    Equipment,
    Consumable
}

[Serializable]
public class ItemData
{
    public string itemID;
    public ItemType itemType;
    public string name;

    public bool isStackable;

    // 장착템
    public EquipmentStats equipmentStats;
    public string passiveID;

    // 소모템
    public TargetType target;
    public List<ItemEffect> itemEffect;
    public int maxStack;
    public int cooltime;
}

[Serializable]
public class EquipmentStats
{
    public int flatHP = 0;
    public int flatMP = 0;
    public int flatAtk = 0;
    public int flatDef = 0;
    public int flatSpeed = 0;

    public float mulHP = 1.0f;
    public float mulMP = 1.0f;
    public float mulAtk = 1.0f;
    public float mulDef = 1.0f;
    public float mulCri = 1.0f;
}

[Serializable]
public class ItemEffect
{
    public string effectID;
    public int value;
    public float mul;
    public int duration;
}

public enum CompareOP
{
    GE, LE, EQ, NE, GT, LT, None
}

[Serializable]
public class Passive
{
    public string passiveID;
    public BattleState timing;

    public string condition;
    public CompareOP op;
    public float condition_value;

    public StatusType stat;
    public float value;
}

[Serializable]
public class PassiveRuntime
{
    public Passive data;
    public bool isActive;
    public string tag;
}

[Serializable]
public class ItemStack
{
    public string itemID;
    public int stack;
}
