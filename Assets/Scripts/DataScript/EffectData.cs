
using System;

public class EffectData
{
    public string effectID;
    public EffectType type;
    public BattleState timing;
    public StackType stack;

    public int duration;     // DoT: 틱 횟수 / 토큰형: 초기 토큰 수
    public int value;        // DoT: 틱당 데미지 / 토큰형 스탯: 정수 퍼센트 / DMGReduce: 정수 퍼센트
    public int maxStack;     // AddDamage 스택 최대치
    public int maxDuration;  // 최대 지속시간

    public string linkedEffectID; // 이 이펙트가 제거될 때 같이 제거할 이펙트 ID

    public CleanMode cleanMode;
    public string cleanType;
}

[Serializable]
public class ActiveEffect
{
    public EffectData data;
    public string sourceTag;
    public int value;    // 런타임 값 (DoT 스택 추적용)
    public int duration;
    public int token;    // 토큰형 effect (Guard, ATKUp/Down, DEFUp/Down, SPDUp/Down)
}

public enum StackType
{
    None,
    ResetDuration,
    AddDamage
}

public enum EffectType
{
    Bleed,      // 턴 시작 데미지, stack
    Poison,     // 턴 종료 데미지 + MP 감소, stack
    Freeze,     // 다음 피격 시 데미지 증가(토큰), reset
    Burn,       // 턴 시작 데미지 + 방어력 감소, stack
    Stun,       // 턴 행동 불가, reset
    Clean,      // 상태이상 제거 (즉발)
    Heal,       // 체력 회복 (즉발)
    RecovoryMP, // MP 회복 (즉발)
    ATKUp,      // 공격력 증가 (토큰: 공격 시 소모)
    ATKDown,    // 공격력 감소 (토큰: 공격 시 소모)
    DEFUp,      // 방어력 증가 (토큰: 피격 시 소모)
    DEFDown,    // 방어력 감소 (토큰: 피격 시 소모)
    SPDUp,      // 속도 증가 (토큰: 라운드 시작 시 소모)
    SPDDown,    // 속도 감소 (토큰: 라운드 시작 시 소모)
    DMGReduce,  // 받는 피해 감소
    Guard,      // 피해 무시 (토큰)
    MPDrain     // MP 감소 (틱)
}

public enum CleanMode
{
    None,
    AllDebuff,
    AllEffectType,
    SelectEffectType
}
