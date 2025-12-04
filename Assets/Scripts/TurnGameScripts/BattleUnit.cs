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

    public int currentHP;
    public int currentMP;

    public bool isDead => currentHP <=0;

    public BattleUnit(CharacterStat stat, TeamType teamType)
    {
        baseStat = stat;
        team = teamType;
        currentHP = stat.hp;
        currentMP = stat.mp;
    }
}
