using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class EnemyAIProfile : ScriptableObject
{
    [Header("추가 행동 횟수")]
    public int bonusMainAction = 0;
    public int bonusSubAction = 0;

    [Header("공격 / 스킬 / 방어 가중치")]
    public float attackWeight;
    public float skillWeight;
    public float defendWeight;

    [Header("타겟 우선순위")]
    public AITargetPolicy targetPolicy;

    
    public abstract AIAction Decide(BattleUnit self, BattleContext ctx);
    protected AICommandType DecideCommandType()
    {
        float total = attackWeight + skillWeight + defendWeight;
        float rnd = UnityEngine.Random.Range(0, total);

        if(rnd < attackWeight) return AICommandType.Attack;
        rnd -= attackWeight;
        
        if(rnd < skillWeight) return AICommandType.Skill;

        return AICommandType.Defend;
    }
    
    protected BattleUnit DecideTarget(BattleUnit self, BattleContext ctx, AICommandType cmd, SkillData skill = null)
    {
        List<BattleUnit> targets;

        if(cmd == AICommandType.Skill && skill != null)
        {
            targets = ctx.SkillRangeTargets(self,skill);
        }
        else
        {
            targets = ctx.AttackRangeTargets(self);
        }

        if(targets == null || targets.Count == 0) return null;

        switch(targetPolicy)
        {
            case AITargetPolicy.LowestHP:
                return targets.OrderBy(t=>t.currentHP).First();
            
            case AITargetPolicy.HighestAttack:
                return targets.OrderByDescending(t=>t.attack).First();
            
            default:
                return targets[UnityEngine.Random.Range(0,targets.Count)];
        }
    }

    protected virtual SkillData DecideSkill(BattleUnit self)
    {
        if(self.skills == null || self.skills.Count == 0) return null;

        var usable = new List<SkillData>();

        foreach(var s in self.skills)
        {
            if(self.CanUseSkill(s.Key))
            {
                usable.Add(s.Value);
                Debug.Log(s.Key);
            }
        }

        if(usable.Count == 0) return null;

        return usable[UnityEngine.Random.Range(0, usable.Count)];
    }
}

public enum AITargetPolicy
{
    Random,
    LowestHP,
    HighestAttack
}

[Serializable]
public class AIPhase
{
    //public float hpRate;
    public float attackWeight;
    public float skillWeight;
    public float defendWeight;
    public int fixedPattern;
}