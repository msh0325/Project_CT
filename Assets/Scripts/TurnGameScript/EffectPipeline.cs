using System.Linq;
using UnityEngine;

public struct EffectEvent
{
    public EffectData baseData;
    public int value;
    public int duration;
    public BattleUnit source;
}

public static class EffectPipeline
{
    public static void ApplyEffectPacket(BattleUnit target, EffectEvent p)
    {
        if(target == null || target.isDead) return;
        if(p.baseData == null) return;

        var data = p.baseData;
        bool isImmediate = data.type == EffectType.Heal || data.type == EffectType.RecovoryMP || data.type == EffectType.Clean;
        bool isToken = IsTokenBased(data.type);

        int resolvedDuration = (p.duration > 0) ? p.duration : data.duration;

        // 토큰형이라도 duration <= 0이면 무한 지속(패시브 버프 등)으로 처리
        bool useToken = isToken && resolvedDuration > 0;

        var ae = new ActiveEffect
        {
            data = data,
            value = (p.value != 0) ? p.value : data.value,
            duration = useToken ? 0 : resolvedDuration,
            token = useToken ? resolvedDuration : 0
        };

        if (isImmediate)
        {
            ae.duration = 0;
            target.TakeEffect(ae);
            return;
        }

        bool buffDirty = AddOrStack(target, ae);
        if(buffDirty) target.CalcBuff();

        // 링크된 이펙트 자동 적용 (아직 없을 때만)
        if(!string.IsNullOrEmpty(data.linkedEffectID))
        {
            bool alreadyLinked = target.activeEffects.Any(e => e.data.effectID == data.linkedEffectID);
            if(!alreadyLinked && DataManager.instance.effectDatas.TryGetValue(data.linkedEffectID, out var linkedData))
            {
                ApplyEffectPacket(target, new EffectEvent { baseData = linkedData, source = p.source });
            }
        }
    }

    public static bool IsTokenBased(EffectType t) =>
        t == EffectType.ATKUp || t == EffectType.ATKDown ||
        t == EffectType.DEFUp || t == EffectType.DEFDown ||
        t == EffectType.SPDUp || t == EffectType.SPDDown ||
        t == EffectType.Guard;

    public static bool IsStatType(EffectType t) =>
        t == EffectType.ATKUp || t == EffectType.ATKDown ||
        t == EffectType.DEFUp || t == EffectType.DEFDown ||
        t == EffectType.SPDUp || t == EffectType.SPDDown;

    public static bool AddOrStack(BattleUnit target, ActiveEffect ae)
    {
        if(target == null) return false;

        var exist = target.activeEffects.FirstOrDefault(e=>e.data.effectID == ae.data.effectID);

        if(exist == null)
        {
            target.activeEffects.Add(ae);
            return IsStatType(ae.data.type) || ae.data.type == EffectType.Guard;
        }

        switch (exist.data.stack)
        {
            case StackType.ResetDuration:
                if(IsTokenBased(exist.data.type))
                    exist.token = ae.token;
                else
                    exist.duration = ae.duration;
                break;

            case StackType.AddDamage:
                if(exist.value < ae.data.maxStack)
                {
                    exist.value = Mathf.Min(exist.value + Mathf.RoundToInt(ae.value / 2f), ae.data.maxStack);
                    exist.duration = Mathf.Min(exist.duration + Mathf.RoundToInt(ae.duration / 2f), ae.data.maxDuration);
                }
                break;
        }

        return IsStatType(ae.data.type);
    }
}
