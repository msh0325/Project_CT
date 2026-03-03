using System;
using UnityEngine;

public abstract class EnemyAIProfile : ScriptableObject
{
    [Header("추가 행동 횟수")]
    public int bonusMainAction = 0;
    public int bonusSubAction = 0;
    public abstract AIAction Decide(BattleUnit self, BattleContext ctx);
}

public enum AITargetPolicy
{
    Random,
    LowestHP,
    HightestAttack
}

[Serializable]
public class AIPhase
{
    public float hpRate;
    public float attackWeight;
    public float skillWeight;
    public float defendWeight;
}