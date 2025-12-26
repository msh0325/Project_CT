using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public static class EffectPipeline
{
    public struct EffectPacket
    {
        public EffectData baseData;
        public int value;
        public float mul;
        public int duration;
        public BattleUnit source;
    }

    public static void ApplyEffectPacket(BattleUnit target, EffectPacket p)
    {
        if(target == null || target.isDead) return;
        if(p.baseData == null) return;

        var data = p.baseData;
        bool isImmediate = data.type == EffectType.Heal || data.type == EffectType.RecovoryMP || data.type == EffectType.Clean;
        bool isStatEffect = data.status != StatusType.None;

        var ae = new ActiveEffect
        {
            data = data,
            damage = (p.value != 0)? p.value:data.damage,
            statMul = (p.mul != 0f)?p.mul:data.statmul,
            duration = (p.duration > 0) ? p.duration : data.duration,
            statEnable = data.applyTiming == ApplyTiming.Immediate,
            token = data.type == EffectType.Guard? 1:0 // 토큰형 테스트용. 추후 수정 필요
        };

        if (isImmediate)
        {
            ae.duration = 0;
            target.TakeEffect(ae);

            if(isStatEffect && ae.statEnable)
            {
                target.CalcBuff();
            }
            return;
        }

        bool buffDirty = AddOrStack(target, ae);

        if(isStatEffect && ae.statEnable) buffDirty = true;

        if(buffDirty) target.CalcBuff();
    }

    public static bool AddOrStack(BattleUnit target, ActiveEffect ae)
    {
        if(target == null) return false;

        var exist = target.activeEffects.FirstOrDefault(e=>e.data.effectID == ae.data.effectID);

        if(exist == null)
        {
            target.activeEffects.Add(ae);
            bool dirty = ae.data.status != StatusType.None;
            if(ae.data.type == EffectType.Guard) dirty = true;
            return dirty;
        }

        bool buffDirty = false;

        switch (exist.data.stack)
        {
            case StackType.None: 
                break;

            case StackType.ResetDuration:
                exist.duration = ae.duration;
                break;

            case StackType.AddDamage:
                if(exist.damage < ae.data.maxDamage)
                {
                    exist.damage = Mathf.Min(exist.damage + Mathf.RoundToInt(ae.damage / 2),ae.data.maxDamage);
                    exist.duration = Mathf.Min(exist.duration + Mathf.RoundToInt(ae.duration / 2), ae.data.maxDuration);
                }
                break;
        }

        if(exist.data.status != StatusType.None) buffDirty = true;

        return buffDirty;
    }
}
