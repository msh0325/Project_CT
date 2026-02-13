using System;
using UnityEngine;

public abstract class EnemyAIProfile : ScriptableObject
{
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