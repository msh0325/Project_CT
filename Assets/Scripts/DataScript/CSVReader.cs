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
    private bool isFinish = false;
    // dictionary를 이용해서 id로 캐릭터 정보 불러오기
    public Dictionary<string, CharacterStat> characterStats = new();
    public Dictionary<string, SkillData> skilldatas = new();

    [ContextMenu("Read CSV File")]
    private void ReadCSV()
    {

        if(csvFile == null)
        {
            Debug.LogError("csvFile이 비어있습니다");
            return;
        }

        string[] lines = csvFile.text.Split('\n);
        if(lines.Length <= 1)
        {
            Debug.LogWarning("csv에 데이터가 없음");
            return;
        }
        // 첫줄은 헤더니까 스킵
        for(int i=1;i<lines.Length;i++)
        {
            string line = lines[u].Trim();
            if(string.IsNullOrWhiteSpace(line)) continue;

            string[] cols = line.Split(',');

            if(readData == ReadData.characterData)
            {
                if(cols.Length < 9)
                { 
                    Debug.LogWarning($"CharacterData 컬럼 개수 부족 : {line}");
                    continue;
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

                if(!characterStats.ContainKey(stat.characterID))
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
                    skillData.Add(data.skillID, data);
                }
            }
        }
    /*
        string fileName = csvFile.name;
        StreamReader reader = new StreamReader(Application.dataPath + "/Data/" + fileName + ".csv");
        isFinish = false;
        reader.ReadLine(); // 첫번째 데이터 카테고리 읽고 버리기

        if (readData == ReadData.characterdata)
        {
            while (isFinish == false)
            {
                string data = reader.ReadLine();

                if (data == null)
                {
                    isFinish = true;
                    break;
                }

                var splitData = data.Split(',');
                CharacterStat stat = new CharacterStat();

                stat.characterID = splitData[0];
                stat.name = splitData[1];
                stat.speed_min = int.Parse(splitData[2]);
                stat.speed_max = int.Parse(splitData[3]);
                stat.hp = int.Parse(splitData[4]);
                stat.mp = int.Parse(splitData[5]);
                stat.attack = int.Parse(splitData[6]);
                stat.defense = int.Parse(splitData[7]);
                string skillstring = splitData[8];
                stat.skillID = skillstring.Split('/', StringSplitOption.RemoveEmptyEntries);

                characterStats.Add(stat.characterID, stat);
                Debug.Log($"stat.name : {stat.name}");
                Debug.Log($"characterStats.Count : {characterStats.Count}");
            }
        }
        else if (readData == ReadData.skilldata)
        {
            Debug.Log("skilldata");
        }*/
    }
}
