using UnityEngine;

public class SkillData
{
    public string skillID {get;set;}
    public string skillName {get;set;}
    public int random_min {get;set;}
    public int random_max {get;set;}
    public float multiplier {get;set;}
    public int useMP {get;set;}
    public int coolTime {get;set;}

    public SkillType skillType;
    public TargetType targetType;
    public RowType[] range;
    public string[] effectID;
}

public enum SkillType
{
    Damage,
    Buff,
    Debuff,
    Heal
}

public enum TargetType
{
    EnemySingle,
    EnemyAll,
    AllySingle,
    AllyAll,
    Self
}