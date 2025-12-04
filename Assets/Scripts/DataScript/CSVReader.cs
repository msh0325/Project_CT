using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class CSVReader : MonoBehaviour
{
    [Header("CSV Data File")]
    public TextAsset characterCSV;
    public TextAsset skillCSV;
    public TextAsset stageCSV;
    public TextAsset waveCSV;
    
    public void ReadCharacterCSV(Dictionary<string, CharacterStat> saveFile)
    {
        if(characterCSV == null)
        {
            Debug.LogError("characterCSVFile이 비어있습니다");
            return;
        }

        string[] lines = characterCSV.text.Split('\n');
        if(lines.Length <= 1)
        {
            Debug.LogWarning("csv 파일에 데이터가 없음");
            return;
        }

        // 첫줄은 헤더니까 스킵
        for(int i=1;i<lines.Length;i++)
        {
            string line = lines[i].Trim();

            if(string.IsNullOrWhiteSpace(line)) continue;

            string[] cols = line.Split(',');

            if(cols.Length < 9)
            {
                Debug.LogWarning($"CharacterData 컬럼 개수 부족");
                continue;
            }
            
            // 문자열의 " 없애기
            for(int j=0;j<cols.Length;j++)
            {
                cols[j] = cols[j].Trim().Trim('"');
            }

            CharacterStat stat = new CharacterStat
            {
                characterID = cols[0],
                name = cols[1],
                speed_min = int.Parse(cols[2]),
                speed_max = int.Parse(cols[3]),
                hp = int.Parse(cols[4]),
                mp = int.Parse(cols[5]),
                attack = int.Parse(cols[6]),
                defense = int.Parse(cols[7]),
                skillID = cols[8].Split('/',StringSplitOptions.RemoveEmptyEntries)
            };

            if(!saveFile.ContainsKey(stat.characterID))
            {
                saveFile.Add(stat.characterID,stat);
            }
        }
    }

    public void ReadSkillCSV(Dictionary<string, SkillData> saveFile)
    {
        if(skillCSV == null)
        {
            Debug.LogError("skillCSVFile이 비어있습니다");
            return;
        }

        string[] lines = skillCSV.text.Split('\n');
        if(lines.Length <= 1)
        {
            Debug.LogWarning("csv 파일에 데이터가 없음");
            return;
        }

        // 첫줄은 헤더니까 스킵
        for(int i=1;i<lines.Length;i++)
        {
            string line = lines[i].Trim();

            if(string.IsNullOrWhiteSpace(line)) continue;

            string[] cols = line.Split(',');

            if(cols.Length < 6)
            {
                Debug.LogWarning($"SkillData 컬럼 개수 부족");
                continue;
            }
            
            // 문자열의 " 없애기
            for(int j=0;j<cols.Length;j++)
            {
                cols[j] = cols[j].Trim().Trim('"');
            }

            SkillData data = new SkillData
            {
                skillID = cols[0],
                random_min = int.Parse(cols[1]),
                random_max = int.Parse(cols[2]),
                multiplier = float.Parse(cols[3]),
                useMP = int.Parse(cols[4]),
                coolTime = int.Parse(cols[5])
            };
            
            if(!saveFile.ContainsKey(data.skillID))
            {
                saveFile.Add(data.skillID,data);
            }
        }
    }

    public void ReadStageCSV(Dictionary<string, StageData> saveFile)
    {
        if(stageCSV == null)
        {
            Debug.LogError("stageCSVFile이 비어있습니다");
            return;
        }

        string[] lines = stageCSV.text.Split('\n');
        if(lines.Length <= 1)
        {
            Debug.LogWarning("csv 파일에 데이터가 없음");
            return;
        }

        // 첫줄은 헤더니까 스킵
        for(int i=1;i<lines.Length;i++)
        {
            string line = lines[i].Trim();

            if(string.IsNullOrWhiteSpace(line)) continue;

            string[] cols = line.Split(',');

            if(cols.Length < 2)
            {
                Debug.LogWarning($"Stage 컬럼 개수 부족");
                continue;
            }
            
            // 문자열의 " 없애기
            for(int j=0;j<cols.Length;j++)
            {
                cols[j] = cols[j].Trim().Trim('"');
            }

            StageData data = new StageData
            {
                stageID = cols[0],
                stageName = cols[1]
            };

            if(!saveFile.ContainsKey(data.stageID))
            {
                saveFile.Add(data.stageID,data);
            }
        }
    }

    public void ReadWaveCSV(Dictionary<string, List<WaveData>> saveFile)
    {
        if(waveCSV == null)
        {
            Debug.LogError("waveCSVFile이 비어있습니다");
            return;
        }

        string[] lines = waveCSV.text.Split('\n');
        if(lines.Length <= 1)
        {
            Debug.LogWarning("csv 파일에 데이터가 없음");
            return;
        }

        // 첫줄은 헤더니까 스킵
        for(int i=1;i<lines.Length;i++)
        {
            string line = lines[i].Trim();

            if(string.IsNullOrWhiteSpace(line)) continue;

            string[] cols = line.Split(',');

            if(cols.Length < 4)
            {
                Debug.LogWarning($"wave 컬럼 개수 부족");
                continue;
            }
            
            // 문자열의 " 없애기
            for(int j=0;j<cols.Length;j++)
            {
                cols[j] = cols[j].Trim().Trim('"');
            }
            
            string[] enemyid = cols[2].Split('/',StringSplitOptions.RemoveEmptyEntries);
            string[] countText = cols[3].Split('/',StringSplitOptions.RemoveEmptyEntries);
            int len = Mathf.Min(enemyid.Length, countText.Length);
            int[] counts = new int[len];
            for(int n=0;n<len;n++)
            {
                counts[n] = int.Parse(countText[n]);
            }


            WaveData data = new WaveData
            {
                stageID = cols[0],
                waveIndex = int.Parse(cols[1]),
                enemyID = enemyid,
                enemyCount = counts
            };

            if(!saveFile.TryGetValue(data.stageID,out var list))
            {
                list = new List<WaveData>();
                saveFile[data.stageID] = list;
            }

            list.Add(data);
        }
    }
}
