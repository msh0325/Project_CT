using UnityEngine;
using System.Collections.Generic;

public class DataManager : MonoBehaviour
{
    public static DataManager instance;

    public CSVReader csvReader;

    // dictionary를 이용해 id로 캐릭터와 스킬 등 게임의 고정 데이터를 불러오기
    public Dictionary<string, CharacterStat> characterStats = new();
    public Dictionary<string, SkillData> skillDatas = new();
    public Dictionary<string, EffectData> effectDatas = new();
    public Dictionary<string, StageData> stageDatas = new();
    public Dictionary<string, List<WaveData>> waveDatas = new();
    public Dictionary<string, SupportData> supportData = new();
    public Dictionary<string, ItemData> itemData = new();
    public Dictionary<string,Passive> passiveData = new();
    public Dictionary<string,EnemyAIProfile> aiProfileData = new();
    public HashSet<string> linkedTargetIDs = new(); // effect 중 하위 effect(ex. 독의 마나감소, 화상의 방어력감소) 관리하기 위한 hashset

    void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        // characterData 불러오기
        csvReader.ReadCharacterCSV(characterStats);

        // skillData 불러오기
        csvReader.ReadSkillCSV(skillDatas);

        // effectData 불러오기
        csvReader.ReadEffectCSV(effectDatas);

        // stageData 불러오기
        csvReader.ReadStageCSV(stageDatas);

        // waveData 불러오기
        csvReader.ReadWaveCSV(waveDatas);

        // supportData 불러오기
        csvReader.ReadSupportCSV(supportData);

        // itemData 불러오기
        csvReader.ReadItemCSV(itemData);

        // itemPassive 불러오기
        csvReader.ReadPassiveCSV(passiveData);

        // AIProfiles 불러오기
        LoadAIProfiles();

        // linkedTargetIDs 저장하기
        SaveLinkedTargetIDs();
    }

    private void LoadAIProfiles()
    {
        aiProfileData.Clear();

        var profiles = Resources.LoadAll<EnemyAIProfile>("AIProfiles");
        foreach(var p in profiles)
        {
            if (!aiProfileData.ContainsKey(p.name))
            {
                aiProfileData.Add(p.name, p);
            }
        }
    }

    public EnemyAIProfile GetAIProfile(string key)
    {
        if(aiProfileData.TryGetValue(key, out var p)) return p;

        Debug.LogWarning($"AIProfileData에 {key} 없음.");
        return aiProfileData["Basic"];
    }

    private void SaveLinkedTargetIDs()
    {
        linkedTargetIDs.Clear();

        foreach(var kv in effectDatas)
        {
            if(!string.IsNullOrEmpty(kv.Value.linkedEffectID))
            {
                linkedTargetIDs.Add(kv.Value.linkedEffectID);
            }
        }
    }
}