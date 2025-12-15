using System.Collections.Generic;
using UnityEngine;

public class EffectData
{
    public string effectID; // 상태이상 ID
    public EffectType type; // 상태이상 종류
    public BattleState timing; // 상태이상 체크 타이밍
    public StackType stack; // 상태이상 스택 종류(지속시간 초기화, 스택/지속시간 증가)
    public int duration; // 상태이상 지속시간
    public int damage; // 상태이상 데미지?
    public int maxDamage; // 상태이상 최대 스택 수 / 버프/디버프는 데미지로 고정?
    public StatusType status; // 버프/디버프 영향주는 스테이터스 / 상태이상은 None
}

public class ActiveEffect
{
    public EffectData data;
    public int damage;
    public int duration;
}

public enum StackType
{
    None,
    ResetDuration,
    AddDamage
}

// 상태이상 종류
public enum EffectType
{
    StatBuff,
    StatDebuff,
    Bleed, // 턴 시작시 데미지
    Poison, // 턴 종료시 데미지
    Freeze, // 다음 턴 속도 감소
    Burn, // 턴 시작시 데미지, 방어력 감소
    Stun // 턴을 제일 마지막으로 미룸.
}

/*public enum EffectTiming
{
    RoundStart,
    TurnStart,
    TurnEnd,
    RoundEnd
}*/

public enum StatusType
{
    None,
    Speed,
    Attack,
    Defense
}