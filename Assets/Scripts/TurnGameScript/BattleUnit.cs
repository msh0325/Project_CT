using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum TeamType
{
    Ally,
    Enemy
}

public class BattleUnit
{
    public CharacterStat baseStat;
    public TeamType team;

    public string name;
    public int baseAttack;
    public int baseDefense;
    public int baseMaxHP;
    public int baseMaxMP;
    public int baseSpeedMin;
    public int baseSpeedMax;

    public int currentHP;
    public int currentMP;
    public int attack => baseAttack + attackBonus;
    public int defense => baseDefense + defenseBonus;
    public int maxHP => baseMaxHP;
    public int maxMP => baseMaxMP;
    public int speed_min => baseSpeedMin + bonusSpeed_min;
    public int speed_max => baseSpeedMax + bonusSpeed_max;
    public int currentSpeed;

    private int attackBonus;
    private int defenseBonus;
    private int bonusSpeed_min;
    private int bonusSpeed_max;

    public List<ActiveEffect> activeEffects = new();

    public PlayerCharacterStat pcCharStat;
    public PartyMemberSetting partyChar;
    public RowType row;

    public Dictionary<string, SkillData> skills = new();
    public Dictionary<string,int> cooldowns = new();

    const int BaseMainAction = 1;
    const int BaseSubAction = 2;
    public int mainActionCount;
    public int leftMainAction;
    public int subActionCount;
    public int leftSubAction;

    public bool isDead => currentHP <= 0;

    public void RollSpeed(System.Random rnd)
    {
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

        bool isBonusNull = bonusStat == null;
        baseAttack = stat.attack + (isBonusNull? 0 : bonusStat.bonusAttack);
        baseDefense = stat.defense + (isBonusNull? 0 : bonusStat.bonusDefense);
        baseMaxHP = stat.hp + (isBonusNull ? 0 : bonusStat.bonusHP);
        baseMaxMP = stat.mp + (isBonusNull ? 0  : bonusStat.bonusMP);
        baseSpeedMin = stat.speed_min;
        baseSpeedMax = stat.speed_max;

        currentHP = baseMaxHP;
        currentMP = baseMaxMP;

        mainActionCount = BaseMainAction + (isBonusNull ? 0 : bonusStat.bonusMainAction);
        subActionCount = BaseSubAction + (isBonusNull ? 0 : bonusStat.bonusSubAction);   
        leftMainAction = mainActionCount;
        leftSubAction = subActionCount;
        row = rowType;
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

    public void TestAttack(BattleUnit target, System.Random rnd)
    {
        int targetDEF = target.defense;
        // (공격력 * 배율) + 랜덤 보정 데미지
        int dmg = Mathf.RoundToInt((attack * 1.5f) + rnd.Next(3,6));

        int realDMG = Mathf.Max(dmg-targetDEF,0);
        target.currentHP = Mathf.Max(target.currentHP - realDMG,0);

        //Debug.Log($"{name}이 {target.name}를 향해 공격. 데미지 : {dmg}, 실제 데미지 : {realDMG}");
        //Debug.Log($"{target.name}의 남은 HP {target.currentHP}");
        //Debug.Log($"{attack}");
    }

    public void ConsumeSkillCost(SkillData skill)
    {
        currentMP -= skill.useMP;
        StartCoolDown(skill);
    }

    public int CalcSkillRealDamage(SkillData skill, System.Random rnd)
    {
        int randomBonus = rnd.Next(skill.random_min, skill.random_max);
        float dmg = (attack * skill.multiplier) + randomBonus;
        return Mathf.RoundToInt(dmg);
    }

    public void TakeDamage(BattleUnit target,SkillData skill, int dmg)
    {
        switch (skill.skillType)
        {
            case SkillType.Damage:
                int realDMG = Mathf.Max(dmg-target.defense,0);
                target.currentHP = Mathf.Max(target.currentHP - realDMG, 0);
                Debug.Log($"{name}이 {target.name}를 향해 공격. 데미지 : {dmg}, 실제 데미지 : {realDMG}");
                Debug.Log($"{target.name}의 남은 HP {target.currentHP}");
                break;

            case SkillType.Buff:
                break;

            case SkillType.Debuff:
                break;

            case SkillType.Heal:
                target.currentHP = Mathf.Min(target.currentHP + dmg, target.baseMaxHP);
                Debug.Log($"{name}이 {target.name}을 힐 : {dmg}");
                break;
        }

        foreach(var id in skill.effectID)
        {
            EffectData effectData = DataManager.instance.effectDatas[id];
            target.ApplyEffect(effectData);
        }        
    }

    private void StartCoolDown(SkillData skill)
    {
        if(skill.coolTime >= 0)
        {
            cooldowns[skill.skillID] = skill.coolTime;
            Debug.Log($"{skill.skillName} : {cooldowns[skill.skillID]}");
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

    public void ApplyEffect(EffectData effect)
    {
        var id = activeEffects.FirstOrDefault(e=>e.data.effectID == effect.effectID);        

        if(id == null)
        {
            activeEffects.Add(new ActiveEffect
            {
                data = effect,
                damage = effect.damage,
                duration = effect.duration
            });
            return;
        }

        switch (id.data.stack)
        {
            case StackType.None: 
                break;

            case StackType.ResetDuration:
                id.duration = effect.duration;
                break;
                
            case StackType.AddDamage:
                if(id.damage < effect.maxDamage)
                {
                    id.damage = Mathf.Min(id.damage + Mathf.RoundToInt(effect.damage / 2),effect.maxDamage);
                    id.duration++;
                }
                break;
        }
    }

    public void CheckEffect(BattleState timing)
    {
        Debug.Log($"timing : {timing}");
        for(int i = activeEffects.Count-1 ; i >=0 ; i--)
        {
            var e = activeEffects[i];

            if(e.data.timing != timing) continue;

            e.duration--;

            if(e.duration < 0)
            {
                activeEffects.RemoveAt(i);
                continue;
            }
            
            TakeEffect(e);
        }
        CalcBuff();
    }

    public void TakeEffect(ActiveEffect e)
    {
        switch (e.data.type)
        {
            case EffectType.Bleed:
            case EffectType.Poison:
            case EffectType.Burn:
                int baseDmg = e.damage;
                currentHP = Mathf.Max(currentHP - baseDmg,0);
                Debug.Log($"{name}가 {e.data.type} 데미지 입음 : {e.damage}, 남은 턴수 {e.duration}");
                break;
            
            case EffectType.StatBuff:
            case EffectType.StatDebuff:
                Debug.Log($"{name}가 {e.data.type} 얻음. 남은 턴수 {e.duration}");
                break;
            
            case EffectType.Freeze:
            case EffectType.Stun:
                break;
        }
    }

    public void CalcBuff()
    {
        attackBonus = 0;
        defenseBonus = 0;
        bonusSpeed_max = 0;

        foreach(var e in activeEffects)
        {
            if(e.data.type != EffectType.StatBuff && e.data.type != EffectType.StatDebuff)
                continue;
            
            int value = e.damage;
            if(e.data.type == EffectType.StatDebuff) 
            {
                value = -value;
            }

            switch (e.data.status)
            {
                case StatusType.None:
                    break;

                case StatusType.Attack:
                    attackBonus += value;
                    break;

                case StatusType.Defense:
                    defenseBonus += value;
                    break;

                case StatusType.Speed:
                    bonusSpeed_max += value;
                    break;
            }
        }
    }
}
