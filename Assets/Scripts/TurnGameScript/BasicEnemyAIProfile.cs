using UnityEngine;

[CreateAssetMenu(menuName ="AI/Basic Enemy AI")]
public class BasicEnemyAIProfile : EnemyAIProfile
{
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
}
