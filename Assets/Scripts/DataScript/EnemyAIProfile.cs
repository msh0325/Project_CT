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