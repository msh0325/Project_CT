using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BattleContext : MonoBehaviour
{
    public int currentWave;
    public int currentRound;

    public List<BattleUnit> allies = new();
    public List<BattleUnit> enemies = new();

    public void NextWave() => currentWave++;
    public void NextRound() => currentRound++;

    public List<BattleUnit> AttackRangeTargets(BattleUnit user)
    {
        List<BattleUnit> source = user.team == TeamType.Ally? enemies:allies;
        var candidates = source.Where(u=>!u.isDead).ToList();
        return candidates;
    }

    // 플레이어의 스킬 타겟 설정
    public List<BattleUnit> SkillRangeTargets(BattleUnit user, SkillData skill)
    {
        List<BattleUnit> source = null;
        bool isAllyteam = user.team == TeamType.Ally;

        switch (skill.targetType)
        {
            case TargetType.EnemySingle:
            case TargetType.EnemyAll:
                source = isAllyteam? enemies:allies;
                break;

            case TargetType.AllySingle:
            case TargetType.AllyAll:
                source = isAllyteam? allies:enemies;
                break;

            case TargetType.Self:
                return new List<BattleUnit>{user};
        }

        var candidates = source.Where(u=>!u.isDead).ToList();

        return candidates;
    }

    public List<BattleUnit> DefenseRangeTarget(BattleUnit user)
    {
        return new List<BattleUnit>{user};
    }

    public List<BattleUnit> ItemRangeTarget(BattleUnit user, ItemData item)
    {
        List<BattleUnit> source = null;
        bool isAllyteam = user.team == TeamType.Ally;

        switch (item.target)
        {
            case TargetType.EnemySingle:
            case TargetType.EnemyAll:
                source =isAllyteam? enemies:allies;
                break;
            
            case TargetType.AllySingle:
            case TargetType.AllyAll:
                source = isAllyteam? allies:enemies;
                break;
            
            case TargetType.Self:
                return new List<BattleUnit>{user};
        }
        var candidates = source.Where(u=>!u.isDead).ToList();
        
        return candidates;
    }
}
