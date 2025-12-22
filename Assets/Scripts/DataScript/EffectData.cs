
using System;

public class EffectData
{
    public string effectID; // 상태이상 ID
    public EffectType type; // 상태이상 종류
    public BattleState timing; // 상태이상 체크 타이밍
    public ApplyTiming applyTiming; // 스탯 버프/디버프 적용 타이밍
    public StackType stack; // 상태이상 스택 종류(지속시간 초기화, 스택/지속시간 증가)

    public int duration; // 상태이상 지속시간
    public int damage; // 상태이상 데미지?
    public int maxDamage; // 상태이상 최대 스택 수 / 버프/디버프는 데미지로 고정?

    public float statmul; // 버프/디버프 배율 값
    public StatusType status; // 버프/디버프 영향주는 스테이터스 / 상태이상은 None
    public CleanMode cleanMode; // 상태이상 제거 모드
    public string cleanType; // 제거할 상태이상 타입
}

[Serializable]
public class ActiveEffect
{
    public EffectData data;
    public int damage;
    public float statMul;
    public int duration;
    public bool statEnable;
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
    Poison, // 턴 종료시 데미지, 마나 감소
    Freeze, // 다음 턴 속도 감소
    Burn, // 턴 시작시 데미지, 방어력 감소
    Stun, // 턴을 제일 마지막으로 미룸
    Clean, // 상태이상 지우기
    Heal, // 체력 회복
    RecovoryMP // mp 회복
}

public enum ApplyTiming
{
    Immediate,
    AfterTick
}

public enum StatusType
{
    None,
    Speed,
    Attack,
    Defense
}

public enum CleanMode
{
    None,
    AllDebuff,
    AllEffectType,
    SelectEffectType
}