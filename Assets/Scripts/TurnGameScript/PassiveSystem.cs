using System;
using UnityEngine;

public enum CompareOP
{
    GE, LE, EQ, NE, GT, LT, None
}

public enum PassiveTrigger
{
    BeforeAction,
    AfterAction,
    BeforeDamageTaken,
    AfterDamageTaken,
    None
}

[Serializable]
public class Passive
{
    public string passiveID;
    public BattleState timing;
    public PassiveTrigger trigger;

    public string condition;
    public CompareOP op;
    public float condition_value;

    public StatusType stat;
    public float value;

    public int applyNextRound; // 패시브를 다음 턴에 적용할지 체크용. 0은 즉시, 1은 다음 턴
}

public class PassiveSystem : MonoBehaviour
{
    public void NotifyTirgger(BattleUnit unit, PassiveTrigger trigger, int currentRound)
    {
        if(unit == null || unit.isDead) return;
        if(unit.passives == null) return;
        foreach(var ps in unit.passives)
        {
            ps.UpdatePassiveTrigger(unit, trigger, currentRound);
        }
    }
}

[Serializable]
public class PassiveRuntime
{
    public Passive data;
    public bool isActive;
    public EffectData passiveEffect;
    public int pendingRound = -1;

    public void UpdatePassive(BattleUnit target, BattleState timing)
    {
        if(target == null || target.isDead) return;
        if(data == null) return;
        if(timing != data.timing) return;

        bool ok = CheckCondition(target);

        if(ok && !isActive)
        {
            isActive = true;
            EffectPipeline.ApplyEffectPacket(target, new EffectEvent
            {
                baseData = passiveEffect,
                value = 0,
                mul = passiveEffect.statmul,
                duration = -1,
                source = null
            });
        }
        else if(!ok && isActive)
        {
            isActive = false;
            int removed = target.activeEffects.RemoveAll(e=> e.data != null && e.data.effectID == passiveEffect.effectID);

            if(removed > 0 && passiveEffect.status != StatusType.None) target.CalcBuff();
        }
    }

    public void UpdatePassiveTrigger(BattleUnit target, PassiveTrigger trigger, int currentRound)
    {
        if(target == null || target.isDead) return;
        if(data == null) return;
        if(trigger != data.trigger) return;

        if(data.applyNextRound <= 0)
        {
            EffectPipeline.ApplyEffectPacket(target, new EffectEvent
            {
                baseData = passiveEffect,
                value = 0,
                mul = passiveEffect.statmul,
                duration = 1,
                source = null
            });
            return;
        }

        // 다음 라운드 적용 예약
        pendingRound = currentRound + data.applyNextRound;
    }

    public bool CheckCondition(BattleUnit target)
    {
        if(target == null) return false;

        float left = -1f;
        switch (data.condition)
        {
            case "hp_rate" :
                left = (target.maxHP <= 0)? 0 : target.currentHP / (float) target.maxHP;
                break;
        }

        float right = data.condition_value;

        return data.op switch
        {
            CompareOP.GE => left >= right,
            CompareOP.LE => left <= right,
            CompareOP.GT => left > right,
            CompareOP.LT => left < right,
            _ => false
        };
    }

    public void TryApplyPendingOnRoundStart(BattleUnit target, int currentRound)
    {
        if(target == null || target.isDead) return;
        if(pendingRound != currentRound) return;

        pendingRound = -1;
        EffectPipeline.ApplyEffectPacket(target, new EffectEvent
        {
            baseData = passiveEffect,
            value = 0,
            mul = passiveEffect.statmul,
            duration = 1,
            source = null
        });
    }
}
