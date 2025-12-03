using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;

[Serializable]
public enum ReadData {CharacterData, SkillData};

public class CSVReader : MonoBehaviour
{
    // 인스펙터 창에서 파일 변경 가능
    public TextAsset csvFile;
    public ReadData readData = ReadData.CharacterData;
    // dictionary를 이용해서 id로 캐릭터 정보 불러오기
    public Dictionary<string, CharacterStat> characterStats = new();
    public Dictionary<string, SkillData> skillDatas = new();

    [ContextMenu("Read CSV File")]
    private void ReadCSV()
    {

        if(csvFile == null)
        {
            Debug.LogError("csvFile이 비어있습니다");
            return;
        }

        string[] lines = csvFile.text.Split('\n');
        if(lines.Length <= 1)
        {
            Debug.LogWarning("csv에 데이터가 없음");
            return;
        }
        // 첫줄은 헤더니까 스킵
        for(int i=1;i<lines.Length;i++)
        {
            string line = lines[i].Trim();
            if(string.IsNullOrWhiteSpace(line)) continue;

            string[] cols = line.Split(',');

            if(readData == ReadData.characterData)
            {
                if(cols.Length < 9)
                { 
                    Debug.LogWarning($"CharacterData 컬럼 개수 부족 : {line}");
                    continue;
                }

                for(int j = 0; j < cols.Length; j++)
                {
                    cols[i] = cols[i].Trim().Trim('"');
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

                if(!characterStats.ContainsKey(stat.characterID))
                {
                    characterStats.Add(stat.characterID, stat);
                }
            }
            else if(readData == ReadData.SkillData)
            {
                if(cols.Length < 6)
                {
                    Debug.LogWarning($"SkillData 컬럼 개수 부족 : {line}");
                    continue;
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

                if(!skillDatas.ContainsKey(data.skillID))
                {
                    skillDatas.Add(data.skillID, data);
                }
            }
        }
    }
}
