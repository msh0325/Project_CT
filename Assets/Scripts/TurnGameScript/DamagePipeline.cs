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
            Debug.Log($"원래 데미지 {dmg}");
            dmg = Mathf.RoundToInt(ev.target.CheckDamageReduce() * dmg);
            Debug.Log($"받는 피해 감소 적용 데미지 {dmg}");
            dmg = Mathf.RoundToInt(ev.target.CheckDamageAmp() * dmg);
            Debug.Log($"받는 피해 증가 적용 데미지 {dmg}");
        }

        ev.target.currentHP = Mathf.Max(ev.target.currentHP - dmg, 0);
        ev.target.ConsumeHitToken();
        passiveSystem?.NotifyTirgger(ev.target,PassiveTrigger.AfterDamageTaken, battleContext.currentRound);
        return dmg;
    }
}