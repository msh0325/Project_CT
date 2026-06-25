using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "AI/Boss AI")]
public class BossAIProfile : EnemyAIProfile
{
    [Header("보스 페이즈 변환 HP / 일정 턴마다 패턴고정")]
    public float changePhaseHPRate;
    public int fixedPattern;

    [Header("보스 패턴 고정 스킬ID")]
    public string patternSkillID;

    private int bossTurnCount = 0;
    private bool isPhase2 = false;

    [Header("2페이즈 변경점")]
    public AIPhase phase2;

    private void OnEnable()
    {
        isPhase2 = false;
        bossTurnCount = 0;
    }

    public override AIAction Decide(BattleUnit self, BattleContext ctx)
    {
        bossTurnCount++;

        if(!isPhase2 && self.currentHP <= self.maxHP * changePhaseHPRate)
        {
            isPhase2 = true;
            // 2페이즈 이후 달라지는 부분 수정
            Debug.Log("2phase start");
            attackWeight = phase2.attackWeight;
            skillWeight = phase2.skillWeight;
            defendWeight = phase2.defendWeight;
            fixedPattern = phase2.fixedPattern;
        }

        if(bossTurnCount % fixedPattern == 0 && self.CanUseSkill(patternSkillID))
        {
            SkillData s = self.skills[patternSkillID];
            return new AIAction
            {
                commandType = AICommandType.Skill,
                skillID = patternSkillID,
                target = DecideTarget(self,ctx,AICommandType.Skill, s)
            };
        }
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
            skillID = skill != null? skill.skillID : null,
            target = target
        };
    }

    protected override SkillData DecideSkill(BattleUnit self)
    {
        if(self.skills == null || self.skills.Count == 0) return null;

        var usable = new List<SkillData>();

        foreach(var s in self.skills)
        {
            if(self.CanUseSkill(s.Key))
            {
                if(s.Key == patternSkillID) continue;
                    
                usable.Add(s.Value);
            }
        }

        if(usable.Count == 0) return null;

        return usable[Random.Range(0,usable.Count)];
    }
}
