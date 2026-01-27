using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName ="AI/Basic Enemy AI")]
public class BasicEnemyAIProfile : EnemyAIProfile
{
    [Header("공격 / 스킬 / 방어 가중치")]
    public float attackWeigt;
    public float skillWeight;
    public float defendWeight;

    [Header("타겟 우선순위")]
    public AITargetPolicy targetPolicy = AITargetPolicy.Random;

    public override AIAction Decide(BattleUnit self, BattleContext ctx)
    {
        AICommandType cmd = DecideCommandType();

        SkillData skill = null;
        if(cmd == AICommandType.Skill)
        {
            skill = DecideSkill(self);
            if(skill == null)
            {
                cmd = AICommandType.Attack;
            }
        }
        BattleUnit target = DecideTarget(self, ctx, cmd, skill);

        return new AIAction
        {
            commandType = cmd,
            skillID = skill != null?skill.skillID : null,
            target = target
        };
    }

    AICommandType DecideCommandType()
    {
        float total = attackWeigt + skillWeight + defendWeight;
        float rnd = Random.Range(0, total);

        if(rnd < attackWeigt) return AICommandType.Attack;
        rnd -= attackWeigt;

        if(rnd < skillWeight) return AICommandType.Skill;
        
        return AICommandType.Defend;
    }

    SkillData DecideSkill(BattleUnit self)
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

        return usable[Random.Range(0, usable.Count)];
    }

    BattleUnit DecideTarget(BattleUnit self, BattleContext ctx, AICommandType cmd, SkillData skill = null)
    {
        List<BattleUnit> targets;

        if(cmd == AICommandType.Skill && skill != null)
        {
            targets = ctx.SkillRangeTargets(self, skill);
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

            case AITargetPolicy.HightestAttack:
                return targets.OrderByDescending(t=>t.attack).First();

            default:
                return targets[Random.Range(0, targets.Count)];
        }
    }
}
