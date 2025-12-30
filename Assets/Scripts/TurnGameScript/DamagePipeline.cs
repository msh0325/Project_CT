using UnityEngine;

public enum DamageKind
{
    Direct,
    Dot,
    True
}

public struct DamageEvent
{
    public BattleUnit source;
    public BattleUnit target;
    public int amount;
    public DamageKind kind;
    public bool allowDamageReduction;
}

public static class DamagePipeline
{
    public static PassiveSystem passiveSystem;
    public static BattleContext battleContext;
    public static void Init(PassiveSystem p, BattleContext b)
    {
        passiveSystem = p;
        battleContext = b;
    }
    public static int Apply(DamageEvent ev)
    {
        if(ev.target == null || ev.target.isDead) return 0;

        int dmg = Mathf.Max(ev.amount, 0);

        if(ev.target.CheckGuardToken()) return 0;

        if (ev.allowDamageReduction)
        {
            dmg = Mathf.RoundToInt(ev.target.CheckDamageReduce() * dmg);
        }

        ev.target.currentHP = Mathf.Max(ev.target.currentHP - dmg, 0);
        passiveSystem?.NotifyTirgger(ev.target,PassiveTrigger.AfterDamageTaken, battleContext.currentRound);
        return dmg;
    }
}