using UnityEngine;

public class EnemyAIController
{
    public AIAction DecideAction(BattleUnit self, BattleContext context)
    {
        var targets = context.AttackRangeTargets(self);

        if(targets.Count == 0)
        {
            return new AIAction();
        }

        var target = targets[Random.Range(0, targets.Count)];

        return new AIAction
        {
            skillID = null,
            target = target
        };
    }
}

public enum AICommandType
{
    Attack,
    Skill,
    Defend
}

public struct AIAction
{
    public AICommandType commandType;
    public string skillID;
    public BattleUnit target;
}
