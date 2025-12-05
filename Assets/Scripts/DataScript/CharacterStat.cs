using UnityEngine;
using System.Collections.Generic;

public class CharacterStat
{
    public string characterID {get;set;}
    public string name {get;set;} 
    public int speed_min {get;set;}
    public int speed_max {get;set;}
    public int hp {get;set;}
    public int mp {get;set;}
    public int attack {get;set;}
    public int defense {get;set;}
    public string[] skillID {get;set;}
}

public class PlayerCharacterStat
{
    public string characterID;

    public int bonusHP;
    public int bonusMP;

    public int bonusAttack;
    public int bonusDefense;

    public List<string> learnedSkillID = new();
    public List<string> defaultEquippedSkillID = new();

    public bool isSelectable = true;
}

public enum RowType
{
    Front = 0,
    Middle = 1,
    Back = 2
}

public class PartyMemberSetting
{
    public string characterID;
    public RowType row;
    public List<string> battleEquippedSkillID = new();
}
