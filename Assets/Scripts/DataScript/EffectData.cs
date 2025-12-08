using System.Collections.Generic;
using UnityEngine;

public class EffectData
{
    public string effectID // 상태이상 ID
    public EffectType type; // 상태이상 종류
    public EffectTiming timing; // 상태이상 체크 타이밍
    public int duration; // 상태이상 지속시간
    public int dmg; // 상태이상 데미지?
}

// 상태이상 종류
public enum EffectType
{
    StatusBuff,
    StatusDebuff,
    Bleed,
    Poison,
    Freeze,
    Burn,
    Stun
}

public enum EffectTiming
{
    RoundStart,
    TurnStart,
    TurnEnd,
    RoundEnd
}