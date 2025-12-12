using UnityEngine;
using System.Collections.Generic;
using System;

public class CharacterStat // 캐릭터 기본 세팅
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

[Serializable]
public class PlayerCharacterStat // 캐릭터의 성장 / 스킬 여부
{
    public string characterID;

    public int bonusHP;
    public int bonusMP;

    public int bonusAttack;
    public int bonusDefense;

    public int bonusMainAction;
    public int bonusSubAction;

    public List<string> learnedSkillID = new();
    public List<string> defaultEquippedSkillID = new();

    public bool isSelectable = true;
}

public enum RowType
{
    Front,
    Middle,
    Back
}

[Serializable]
public class PartyMemberSetting // 편성된 캐릭터 위치 / 선택 스킬
{
    public string characterID;
    public RowType row;
    public List<string> battleEquippedSkillID = new();
}
