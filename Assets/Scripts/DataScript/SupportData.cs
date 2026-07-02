using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SupportData
{
    public string supportID;
    public string name;
    public string activeSkillID;
    public string passiveSkillID;
    public int attack;
    public CastType cast;
}

[Serializable]
public class SupportUnit
{
    public SupportData data;
    public SkillData supportSkill;
    public Passive supportPassive;
    public PassiveRuntime passiveRuntime;
    public int cooldown;

    public static SupportUnit TryCreate(PlayerData pc, DataManager dm)
    {
        var select = pc.selectedSupport;
        if(select == null || string.IsNullOrEmpty(select.supportID))
        {
            Debug.LogWarning("선택된 서포트 없음");
            return null;
        }

        if(!dm.supportData.TryGetValue(select.supportID,out var supportData))
        {
            Debug.LogWarning($"supportdata에 {select.supportID} 없음.");
            return null;
        }

        var unit = new SupportUnit{ data = supportData };

        if(supportData.cast == CastType.Active)
        {
            if (string.IsNullOrEmpty(supportData.activeSkillID))
            {
                Debug.LogWarning($"supportdata에 activeskillid 비어있음 {supportData.supportID}");
                return null;
            }

            if(!dm.skillDatas.TryGetValue(supportData.activeSkillID,out var skill))
            {
                Debug.LogWarning($"skilldatas에 {supportData.activeSkillID} 없음");
                return null;
            }

            unit.supportSkill = skill;
            return unit;
        }
        
        if(supportData.cast == CastType.Passive)
        {
            if (string.IsNullOrEmpty(supportData.passiveSkillID))
            {
                Debug.LogWarning($"supportdata에 passiveskillid 비어있음 {supportData.passiveSkillID}");
                return null;
            }

            if(!dm.passiveData.TryGetValue(supportData.passiveSkillID,out var passive))
            {
                Debug.LogWarning($"passivedata에 {supportData.passiveSkillID} 없음");
                return null;
            }

            unit.supportPassive = passive;
            return unit;
        }

        Debug.LogWarning($"supportdata casttype 확인 필요");
        return null;
    }

    public void ApplySupportPassive(List<BattleUnit> targets)
    {
        if(data == null || supportPassive == null) return;
        if(data.cast != CastType.Passive) return;

        EffectData ed = new()
        {
            effectID = $"PASSIVE_" + data.supportID,
            type = supportPassive.stat,
            timing = BattleState.RoundStart,
            stack = StackType.None,
            duration = -1,
            value = Mathf.RoundToInt(supportPassive.value)
        };

        PassiveRuntime pr = new()
        {
            data = supportPassive,
            passiveEffect = ed
        };
        
        passiveRuntime = pr;

        foreach(var t in targets)
        {
            t.passives.Add(pr);
        }
    }

    public void StartCooldown()
    {
        cooldown = supportSkill.coolTime;
    }
    public void TickCoolDown()
    {
        cooldown = Mathf.Max(cooldown-1,0);
    }
    
    public int CalcDamage(BattleUnit target)
    {
        float rndBonus = UnityEngine.Random.Range(supportSkill.random_min,supportSkill.random_max);
        float power = supportSkill.multiplier * rndBonus * data.attack;
        float ratio = data.attack/(float)(data.attack + target.defense);
        int dmg = Mathf.RoundToInt(power * ratio);
        return Mathf.Max(dmg, 0);
    }

    public bool CanUseSupport()
    {
        if(cooldown > 0) return false;
        else return true;
    }
}

public enum CastType
{
    Active,
    Passive
}
