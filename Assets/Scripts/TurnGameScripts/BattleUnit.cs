using UnityEngine;

public enum TeamType
{
    Ally,
    Enemy
}

public class BattleUnit
{
    public CharacterStat baseStat;
    public TeamType team;

    public int attack;
    public int defense;

    public int currentHP;
    public int currentMP;
    public int currentSpeed;

    public List<SkillData> skills = new();

    public bool isDead => currentHP <=0;

    public void RollSpeed(System.Random rnd)
    {
        currentspeed = rnd.Next(stat.speed_min,stat.speed_max+1);
    }

    public BattleUnit(CharacterStat stat, TeamType teamType)
    {
        baseStat = stat;
        team = teamType;
        attack = stat.attack;
        defense = stat.defense;
        currentHP = stat.hp;
        currentMP = stat.mp;
    }

    public void InitSkills(Dictionary<string, SkillData> skillDB)
    {
        // 캐릭터의 스킬 세팅 미리하기
        // 적 유닛은 skill 전체를 로딩하면 되지만
        // 플레이어 유닛은 모든 스킬중 일부만 선택해서 전투하기 때문에 다른 방식 필요
    }
}
