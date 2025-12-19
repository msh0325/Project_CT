using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

public class CSVReader : MonoBehaviour
{
    [Header("CSV Data File")]
    public TextAsset characterCSV;
    public TextAsset skillCSV;
    public TextAsset effectCSV;
    public TextAsset stageCSV;
    public TextAsset waveCSV;
    public TextAsset supportCSV;
    
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
                Debug.LogWarning("CharacterData 컬럼 개수 부족");
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

            if(cols.Length < 11)
            {
                Debug.LogWarning("SkillData 컬럼 개수 부족");
                continue;
            }
            
            // 문자열의 " 없애기
            for(int j=0;j<cols.Length;j++)
            {
                cols[j] = cols[j].Trim().Trim('"');
            }

            if(!Enum.TryParse<SkillType>(cols[7],out var skType))
            {
                Debug.LogWarning($"skilldata 파싱 실패 : {cols[7]}(line : {line})");
                continue;
            }

            if(!Enum.TryParse<TargetType>(cols[8],out var tgType))
            {
                Debug.LogWarning($"targetdata 파싱 실패 : {cols[8]}(line : {line})");
                continue;
            }

            string[] rowsText = cols[9].Split("/",StringSplitOptions.RemoveEmptyEntries);

            RowType[] rows = new RowType[rowsText.Length];

            for(int n = 0; n < rowsText.Length; n++)
            {
                string text = rowsText[n].Trim();
                
                if(!Enum.TryParse<RowType>(text,out var row))
                {
                    Debug.LogWarning($"rowtype 파싱 실패 : {text}(line : {line})");
                    continue;
                }
                rows[n] = row;
            }

            SkillData data = new SkillData
            {
                skillID = cols[0],
                skillName = cols[1],
                random_min = float.Parse(cols[2]),
                random_max = float.Parse(cols[3]),
                multiplier = float.Parse(cols[4]),
                useMP = int.Parse(cols[5]),
                coolTime = int.Parse(cols[6]),
                skillType = skType,
                targetType = tgType,
                range = rows,
                effectID = cols[10].Split("/",StringSplitOptions.RemoveEmptyEntries)
            };

            if(!saveFile.ContainsKey(data.skillID))
            {
                saveFile.Add(data.skillID,data);
            }
        }
    }

    public void ReadEffectCSV(Dictionary<string, EffectData> saveFile)
    {
        if(effectCSV == null)
        {
            Debug.LogError("effectCSVFile이 비어있습니다");
            return;
        }

        string[] lines = effectCSV.text.Split('\n');
        if(lines.Length <=1)
        {
            Debug.LogWarning("csv 파일에 데이터가 없음");
            return;
        }

        for(int i=1;i<lines.Length;i++)
        {
            string line = lines[i].Trim();

            if(string.IsNullOrWhiteSpace(line)) continue;

            string[] cols = line.Split(',');

            if(cols.Length < 10)
            {
                Debug.LogWarning("Effect 컬럼 개수 부족");
                continue;
            }

            for(int j=0;j<cols.Length;j++)
            {
                cols[j] = cols[j].Trim().Trim('"');
            }

            if(!Enum.TryParse<EffectType>(cols[1],out var effectType))
            {
                Debug.LogWarning($"effectType 파싱 실패 : {cols[1]}(line : {line})");
                continue;
            }

            if(!Enum.TryParse<BattleState>(cols[2],out var effectTiming))
            {
                Debug.LogWarning($"effectTiming 파싱 실패 : {cols[2]}(line : {line})");
                continue;
            }

            if(!Enum.TryParse<ApplyTiming>(cols[3],out var apply))
            {
                Debug.LogWarning($"applytiming 파싱 실패 : {cols[3]}(line:{line})");
                continue;
            }

            if(!Enum.TryParse<StackType>(cols[4],out var stackType))
            {
                Debug.LogWarning($"stacktype 파싱 실패 : {cols[4]}(line:{line})");
                continue;
            }

            if(!Enum.TryParse<StatusType>(cols[9],out var statusType))
            {
                Debug.LogWarning($"statustype 파싱 실패 : {cols[9]}(line:{line})");
                continue;
            }

            EffectData data = new EffectData
            {
                effectID = cols[0],
                type = effectType,
                timing = effectTiming,
                applyTiming = apply,
                stack = stackType,
                duration = int.Parse(cols[5]),
                damage = int.Parse(cols[6]),
                maxDamage = int.Parse(cols[7]),
                statmul = float.Parse(cols[8]),
                status = statusType
            };

            if(!saveFile.ContainsKey(data.effectID))
            {
                saveFile.Add(data.effectID,data);
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
                Debug.LogWarning("Stage 컬럼 개수 부족");
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

            if(cols.Length < 5)
            {
                Debug.LogWarning("wave 컬럼 개수 부족");
                continue;
            }
            
            // 문자열의 " 없애기
            for(int j=0;j<cols.Length;j++)
            {
                cols[j] = cols[j].Trim().Trim('"');
            }
            
            string[] enemyid = cols[2].Split('/',StringSplitOptions.RemoveEmptyEntries);
            string[] countText = cols[3].Split('/',StringSplitOptions.RemoveEmptyEntries);
            string[] rowText = cols[4].Split('/', StringSplitOptions.RemoveEmptyEntries);
            int len = Mathf.Min(enemyid.Length, countText.Length);
            int[] counts = new int[len];
            for(int n=0;n<len;n++)
            {
                counts[n] = int.Parse(countText[n]);
            }

            RowType[] rows = new RowType[rowText.Length];

            for(int n = 0; n < rowText.Length; n++)
            {
                string text = rowText[n].Trim();
                if(!Enum.TryParse<RowType>(text,out var row))
                {
                    Debug.LogWarning($"rowtype 파싱 실패 : {rowText}(line : {line})");
                    continue;
                }
                rows[n] = row;
            }
            int enemyCount = counts.Sum();
            if(rows.Length != enemyCount)
            {
                Debug.LogWarning($"enemyCount랑 enemyRow 수 불일치 : {rows.Length} / {enemyCount}\n"
                + $"wave index : {cols[1]}");
            }
            
            WaveData data = new WaveData
            {
                stageID = cols[0],
                waveIndex = int.Parse(cols[1]),
                enemyID = enemyid,
                enemyCount = counts,
                enemyRow = rows
            };

            if(!saveFile.TryGetValue(data.stageID,out var list))
            {
                list = new List<WaveData>();
                saveFile[data.stageID] = list;
            }

            list.Add(data);
        }
    }

    public void ReadSupportCSV(Dictionary <string,SupportData> saveFile)
    {
        if(supportCSV == null)
        {
            Debug.LogError("supportCSV가 비어있습니다");
            return;
        }

        string[] lines = supportCSV.text.Split('\n');
        if(lines.Length <= 1)
        {
            Debug.LogWarning("csv 파일에 데이터가 없음");
            return;
        }

        for(int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();

            if(string.IsNullOrEmpty(line)) continue;

            string[] cols = line.Split(',');

            if(cols.Length < 5)
            {
                Debug.LogWarning($"support 컬럼 개수 부족(line : {line})");
                continue;
            }

            for(int j = 0; j < cols.Length; j++)
            {
                cols[j] = cols[j].Trim().Trim('"');
            }

            if(!Enum.TryParse<CastType>(cols[4], out var castType))
            {
                Debug.LogWarning($"casttype 파싱 실패 {cols[4]} (line:{line})");
                continue;
            }

            SupportData data = new SupportData
            {
                supportID = cols[0],
                name = cols[1],
                supportSkillID = cols[2],
                attack = int.Parse(cols[3]),
                cast = castType
            };

            if(!saveFile.ContainsKey(data.supportID))
            {
                saveFile.Add(data.supportID,data);
            }
        }
    }
}
