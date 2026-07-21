using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum TeamType
{
    Ally,
    Enemy
}

[Serializable]
public class BattleUnit
{
    public CharacterStat baseStat;
    public TeamType team;
    public EnemyAIProfile profile; // 적 ai

    public string name;
    public int baseAttack;
    public int baseDefense;
    public float baseCritical;
    public int baseMaxHP;
    public int baseMaxMP;
    public int baseSpeedMin;
    public int baseSpeedMax;

    public int currentHP;
    public int currentMP;
    public int attack => Mathf.RoundToInt(baseAttack + attackBonus);
    public int defense => Mathf.RoundToInt(baseDefense + defenseBonus);
    public float critical => Mathf.Clamp(baseCritical + criticalBonus,0,0.9f); // 크리티컬 확률 90퍼까지
    public int maxHP => baseMaxHP;
    public int maxMP => baseMaxMP;
    public int speed_min => baseSpeedMin + bonusSpeed_min;
    public int speed_max => baseSpeedMax + bonusSpeed_max;
    public int currentSpeed;

    [SerializeField] private int attackBonus;
    [SerializeField] private int defenseBonus;
    public float criticalBonus;
    private int bonusSpeed_min;
    private int bonusSpeed_max;

    public List<ActiveEffect> activeEffects = new();

    public PlayerCharacterStat pcCharStat;
    public PartyMemberSetting partyChar;
    public RowType row;

    public Dictionary<string, SkillData> skills = new();
    public Dictionary<string,int> cooldowns = new();
    public List<PassiveRuntime> passives = new();

    public EffectData defend = new();

    const int BaseMainAction = 1;
    const int BaseSubAction = 2;
    public int mainActionCount;
    public int leftMainAction;
    public int subActionCount;
    public int leftSubAction;

    public bool isDead => currentHP <= 0;

    public void RollSpeed(System.Random rnd)
    {
        Debug.Log($"{name} 최소 : {speed_min} 최대 : {speed_max}");
        currentSpeed = rnd.Next(speed_min,speed_max+1);
    }

    public BattleUnit(CharacterStat stat, TeamType teamType, RowType rowType,
     PlayerCharacterStat bonusStat = null, PartyMemberSetting partyMem = null)
    {
        baseStat = stat;
        team = teamType;
        name = stat.name;
        pcCharStat = bonusStat;
        partyChar = partyMem;

        profile = team == TeamType.Ally? null : DataManager.instance.GetAIProfile(stat.aiProfileKey);

        bool isBonusNull = bonusStat == null;
        bool isProfileNull = profile == null;

        baseAttack = stat.attack + (isBonusNull? 0 : bonusStat.bonusAttack);
        baseDefense = stat.defense + (isBonusNull? 0 : bonusStat.bonusDefense);
        baseCritical = stat.critical + (isBonusNull? 0: bonusStat.bonusCritical);
        baseMaxHP = stat.hp + (isBonusNull ? 0 : bonusStat.bonusHP);
        baseMaxMP = stat.mp + (isBonusNull ? 0  : bonusStat.bonusMP);
        baseSpeedMin = stat.speed_min;
        baseSpeedMax = stat.speed_max;

        currentHP = baseMaxHP;
        currentMP = baseMaxMP;

        int bonusMain = 0;
        int bonusSub = 0;

        if(!isBonusNull)
        {
            bonusMain = bonusStat.bonusMainAction;
            bonusSub =  bonusStat.bonusSubAction;
        }
        else if(!isProfileNull)
        {
            bonusMain =  profile.bonusMainAction;
            bonusSub = profile.bonusSubAction;
        }

        mainActionCount = BaseMainAction + bonusMain;
        subActionCount = BaseSubAction + bonusSub;

        leftMainAction = mainActionCount;
        leftSubAction = subActionCount;
        row = rowType;
        //EF_DEFEND_01
        string defid = "EF_DEFEND_BASIC";
        if(DataManager.instance.effectDatas.TryGetValue(defid,out var def))
        {
            defend = def;
        }
    }

    public void InitSkills(Dictionary<string, SkillData> skillDB)
    {
        // 캐릭터의 스킬 세팅 미리하기
        // 적 유닛은 skill 전체를 로딩하면 되지만
        // 플레이어 유닛은 모든 스킬중 일부만 선택해서 전투하기 때문에 다른 방식 필요
        skills.Clear();
        
        if(team == TeamType.Ally)
        {
            if(partyChar == null || partyChar.battleEquippedSkillID == null)
            {
                Debug.LogWarning($"Ally {name} 의 partyChar 또는 battleEquipSkill이 없음");
                return;
            }
            // 플레이어가 선택한 스킬만 로딩
            foreach(string id in partyChar.battleEquippedSkillID)
            {
                if(skillDB.TryGetValue(id,out var skill))
                {
                    skills.Add(skill.skillID, skill);
                }
                else
                {
                    Debug.LogWarning($"스킬 ID {id}을 SKillDB에서 찾을 수 없음");
                }
            }
        }
        else
        {
            if(baseStat.skillID == null || baseStat.skillID.Length <= 1) return;
            
            // basestat에 있는 모든 스킬 로딩
            foreach(string id in baseStat.skillID)
            {
                if (skillDB.ContainsKey(id))
                {
                    skills.Add(id, skillDB[id]);
                }
            }
        }  
    }

    // 현재 턴 / 다음 턴 표시용 이벤트. 추후에 턴 시작 이펙트나 애니메이션 표시 할 때도 사용 할지도
    public Action<bool, bool> OnTurnStateChange;

    public void SetTurnStatus(bool isCurrent, bool isNext)
    {
        OnTurnStateChange?.Invoke(isCurrent, isNext);
    }

    public void OnTurnStart()
    {
        leftMainAction = mainActionCount;
        leftSubAction = Mathf.Min(leftSubAction+1,subActionCount);
    }

    public bool CanUseSkill(string skillID)
    {
        // mp나 cooltime이 부족할 때 사용 못하도록 체크용도
        var skill = skills[skillID];
        if(currentMP < skill.useMP)
        {
            Debug.Log("mp 부족");
            return false;
        }
        
        if(cooldowns.TryGetValue(skillID,out int cd) && cd > 0)
        {
            Debug.Log($"cooldown 중 : {cd}");
            return false;
        }

        return true;
    }

    public bool CanUseMainAction()
    {
        if(leftMainAction <= 0)
        {
            return false;
        }
        return true;
    }

    public bool CanUseSubAction()
    {
        if(leftSubAction <= 0)
        {
            Debug.Log("subaction 사용 횟수 모두 소모");
            return false;
        }
        return true;
    }

    public void UseMainAction()
    {
        leftMainAction--;
    }

    public void UseSubAction()
    {
        leftSubAction--;
    }

    public void Attack(BattleUnit target)
    {
        float criMul = UnityEngine.Random.Range(0,100f) < critical ? 1.5f : 1.0f;
        float rndMul = UnityEngine.Random.Range(1f,1.2f);

        float power = attack * rndMul * criMul;
        int realDMG = CalcRealDamage(power, target);

        ApplyDirectDamage(target, realDMG);
        currentMP = Mathf.Min(currentMP + 10, baseMaxMP);
    }

    public int CalcRealDamage(float power, BattleUnit target)
    {
        float ratio = attack / (float)(attack + target.defense);
        int real = Mathf.RoundToInt(power * ratio);
        return Mathf.Max(real, 0);
    }

    public void ApplyDirectDamage(BattleUnit target, int dmg)
    {
        DamagePipeline.Apply(new DamageEvent
        {
            source = this,
            target = target,
            amount = dmg,
            kind = DamageKind.Direct,
            allowDamageReduction = true
        });
    }

    public void ApplyHeal(int amount)
    {
        if(isDead) return;
        currentHP = Mathf.Min(currentHP + Mathf.Max(amount, 0), maxHP);
    }

    public void ConsumeSkillCost(SkillData skill)
    {
        currentMP -= skill.useMP;
        StartCoolDown(skill);
    }

    public float CalcSkillRealDamage(SkillData skill)
    {
        float criMul = UnityEngine.Random.Range(0,100f) < critical ? 1.5f : 1.0f;
        float rndMul = UnityEngine.Random.Range(skill.random_min, skill.random_max);

        float power = attack * criMul * rndMul;
        return power;
    }

    public void TakeDamage(BattleUnit target,SkillData skill, float power)
    {
        switch (skill.skillType)
        {
            case SkillType.Damage:
                int realDMG = CalcRealDamage(power, target);
                
                ApplyDirectDamage(target, realDMG);
                Debug.Log($"{name}이 {target.name}를 향해 공격. 데미지 : {realDMG}");
                Debug.Log($"{target.name}의 남은 HP {target.currentHP}");
                break;

            case SkillType.Buff:
                Debug.Log($"{name}이 {target.name}에게 버프");
                break;

            case SkillType.Debuff:
                Debug.Log($"{name}이 {target.name}에게 디버프");
                break;

            case SkillType.Heal:
                Debug.Log($"{name}이 {target.name}에게 힐");
                break;
        }

        foreach(var id in skill.effectID)
        {
            if(!DataManager.instance.effectDatas.TryGetValue(id, out var effectData))
            {
                Debug.LogWarning($"effectdatas에 {id} 없음");
                continue;
            }

            EffectEvent ev = new()
            {
                baseData = effectData,
                source = this
            };
            EffectPipeline.ApplyEffectPacket(target, ev);
        }
    }

    public bool CheckGuardToken()
    {
        var guard = activeEffects.FirstOrDefault(e=>e.data.type == EffectType.Guard);
        if(guard == null) return false;
        else
        {
            guard.token--;
            if(guard.token <= 0)
            {
                activeEffects.Remove(guard);
            }
            return true;            
        }
    }

    public float CheckDamageReduce()
    {
        float mul = 1f;
        foreach(var e in activeEffects)
        {
            if(e.data.type != EffectType.DMGReduce) continue;
            mul = Mathf.Min(mul, 1f - e.data.value * 0.01f);
        }
        return mul;
    }

    // freeze 피격 시 데미지 증가 적용 후 제거 (linkedEffectID로 SPDDown도 같이 제거)
    public float CheckDamageAmp()
    {
        float mul = 1f;
        bool dirty = false;
        for(int i = activeEffects.Count - 1; i >= 0; i--)
        {
            var e = activeEffects[i];
            if(e.data.type != EffectType.Freeze) continue;
            Debug.Log("빙결로 인해 받피뎀 증가");
            mul = Mathf.Max(mul, 1f + e.data.value * 0.01f);
            activeEffects.RemoveAt(i);
            RemoveLinkedEffect(e.data.linkedEffectID, ref dirty);
        }
        if(dirty) CalcBuff();
        return mul;
    }

    private void RemoveLinkedEffect(string linkedID, ref bool buffDirty)
    {
        if(string.IsNullOrEmpty(linkedID)) return;
        int removed = activeEffects.RemoveAll(e => e.data.effectID == linkedID);
        if(removed > 0 && activeEffects.Any(e => EffectPipeline.IsStatType(e.data.type)))
            buffDirty = true;
    }

    public void ConsumeAttackToken()
    {
        bool dirty = false;
        for(int i = activeEffects.Count - 1; i >= 0; i--)
        {
            var e = activeEffects[i];
            if(e.data.type != EffectType.ATKUp && e.data.type != EffectType.ATKDown) continue;
            e.token--;
            if(e.token <= 0) { activeEffects.RemoveAt(i); dirty = true; }
        }
        if(dirty) CalcBuff();
    }

    public void ConsumeHitToken()
    {
        bool dirty = false;
        for(int i = activeEffects.Count - 1; i >= 0; i--)
        {
            var e = activeEffects[i];
            if(e.data.type != EffectType.DEFUp && e.data.type != EffectType.DEFDown) continue;
            e.token--;
            if(e.token <= 0) { activeEffects.RemoveAt(i); dirty = true; }
        }
        if(dirty) CalcBuff();
    }


    private void StartCoolDown(SkillData skill)
    {
        if(skill.coolTime >= 0)
        {
            cooldowns[skill.skillID] = skill.coolTime;
        }
    }

    public void TickCoolDown()
    {
        var keys = cooldowns.Keys.ToList();

        foreach(var key in keys)
        {
            cooldowns[key] = Mathf.Max(cooldowns[key]-1,0);
        }
    }

    public int GetSkillCoolTime(SkillData skill)
    {
        if(cooldowns.TryGetValue(skill.skillID,out var cooltime))
        {
            return cooltime;
        }
        return -1;
    }

    public void CheckEffect(BattleState timing)
    {
        bool buffDirty = false;

        for(int i = activeEffects.Count-1; i >= 0; i--)
        {
            var e = activeEffects[i];

            if(e.data.timing != timing) continue;

            TakeEffect(e);

            if(e.token > 0)
            {
                e.token--;
                if(e.token <= 0)
                {
                    activeEffects.RemoveAt(i);
                    RemoveLinkedEffect(e.data.linkedEffectID, ref buffDirty);
                    if(EffectPipeline.IsStatType(e.data.type)) buffDirty = true;
                }
            }
            else if(e.duration >= 0) // duration < 0 = 무한
            {
                e.duration--;
                if(e.duration <= 0)
                {
                    activeEffects.RemoveAt(i);
                    RemoveLinkedEffect(e.data.linkedEffectID, ref buffDirty);
                    if(EffectPipeline.IsStatType(e.data.type)) buffDirty = true;
                    continue;
                }
            }
        }
        if(buffDirty) CalcBuff();
    }

    public void TakeEffect(ActiveEffect e)
    {
        switch (e.data.type)
        {
            case EffectType.Bleed:
            case EffectType.Burn:
                DamagePipeline.Apply(new DamageEvent
                {
                    target = this,
                    amount = e.value,
                    kind = DamageKind.Dot,
                    allowDamageReduction = false
                });
                Debug.Log($"{name}가 {e.data.type} 데미지 입음 : {e.value}, 남은 턴수 {e.duration}");
                break;

            case EffectType.Poison:
                DamagePipeline.Apply(new DamageEvent
                {
                    target = this,
                    amount = e.value,
                    kind = DamageKind.Dot,
                    allowDamageReduction = false
                });
                Debug.Log($"{name}가 {e.data.type} 데미지 입음 : {e.value}, 남은 턴수 {e.duration}");
                break;

            case EffectType.MPDrain:
                currentMP = Mathf.Max(currentMP - e.data.value, 0);
                Debug.Log($"{name} MP 감소 : {e.data.value}, 현재 MP {currentMP}");
                break;

            case EffectType.Freeze:
                Debug.Log($"{name}가 {e.data.type} 얻음. 속도 느려짐. 받는 데미지 증가. 남은 턴수 {e.duration}");
                break;

            case EffectType.Stun:
                leftMainAction = 0;
                Debug.Log($"{name}가 {e.data.type} 얻음. 턴 스킵. 남은 턴수 {e.duration}");
                break;

            case EffectType.Heal:
                ApplyHeal(e.value);
                Debug.Log($"{name} : {e.value} 힐");
                break;

            case EffectType.RecovoryMP:
                currentMP = Mathf.Min(currentMP + e.value, maxMP);
                break;
        }
    }

    public void CalcBuff()
    {
        attackBonus = 0;
        defenseBonus = 0;
        bonusSpeed_max = 0;
        bonusSpeed_min = 0;

        foreach(var e in activeEffects)
        {
            // 토큰형: token > 0 / 지속시간형: duration != 0 (duration=-1 = 무한)
            bool isActive = e.token > 0 || e.duration != 0;
            if(!isActive) continue;

            switch(e.data.type)
            {
                case EffectType.ATKUp:
                    attackBonus += Mathf.RoundToInt(baseAttack * e.data.value * 0.01f);
                    break;
                case EffectType.ATKDown:
                    attackBonus -= Mathf.RoundToInt(baseAttack * e.data.value * 0.01f);
                    break;
                case EffectType.DEFUp:
                    defenseBonus += Mathf.RoundToInt(baseDefense * e.data.value * 0.01f);
                    break;
                case EffectType.DEFDown:
                    defenseBonus -= Mathf.RoundToInt(baseDefense * e.data.value * 0.01f);
                    break;
                case EffectType.SPDUp:
                    bonusSpeed_min += e.data.value;
                    bonusSpeed_max += e.data.value;
                    break;
                case EffectType.SPDDown:
                    bonusSpeed_min += e.data.value; // value는 음수
                    bonusSpeed_max += e.data.value;
                    break;
            }
        }
    }
}
