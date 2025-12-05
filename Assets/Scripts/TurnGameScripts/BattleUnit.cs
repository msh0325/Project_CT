using System.Collections.Generic;
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

    public string name;
    public int attack;
    public int defense;

    public int currentHP;
    public int currentMP;
    public int currentSpeed;

    public List<SkillData> skills = new();

    public bool isDead => currentHP <=0;

    public void RollSpeed(System.Random rnd)
    {
        currentSpeed = rnd.Next(baseStat.speed_min,baseStat.speed_max+1);
    }

    public BattleUnit(CharacterStat stat, TeamType teamType)
    {
        baseStat = stat;
        team = teamType;
        name = stat.name;
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
        foreach(string id in baseStat.skillID)
        {
            if (skillDB.ContainsKey(id))
            {
                skills.Add(skillDB[id]);
            }
        }
    }

    public void TestAttack(BattleUnit target, System.Random rnd)
    {
        int targetDEF = target.defense;
        // (랜덤 보정 데미지 + 공격력) * 배율
        int dmg = Mathf.RoundToInt((rnd.Next(3,6) + attack) * 1.5f);

        int realDMG = Mathf.Max(dmg-targetDEF,0);
        target.currentHP = Mathf.Max(target.currentHP - realDMG,0);

        Debug.Log($"{name}이 {target.name}를 향해 공격. 데미지 : {dmg}, 실제 데미지 : {realDMG}");
        Debug.Log($"{target.name}의 남은 HP {target.currentHP}");
    }
}
