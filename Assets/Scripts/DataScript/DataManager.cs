using UnityEngine;
using System.Collections.Generic;

public class DataManager : MonoBehaviour
{
    public static DataManager instance;

    public CSVReader csvReader;

    // dictionary를 이용해 id로 캐릭터와 스킬 불러오기
    public Dictionary<string, CharacterStat> characterStats = new();
    public Dictionary<string, SkillData> skillDatas = new();
    public Dictionary<string, EffectData> effectDatas = new();
    public Dictionary<string, StageData> stageDatas = new();
    public Dictionary<string, List<WaveData>> waveDatas = new();
    public Dictionary<string, SupportCharacterData> supportData = new();

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

        csvReader.ReadSupportCSV(supportData);
    }
}