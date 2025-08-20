using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;

[Serializable]
public enum ReadData {characterdata, skilldata};

public class CSVReader : MonoBehaviour
{
    // 인스펙터 창에서 파일 변경 가능
    public TextAsset csvFile;
    public ReadData readData = ReadData.characterdata;
    private bool isFinish = false;
    // dictionary를 이용해서 id로 캐릭터 정보 불러오기
    public Dictionary<string, CharacterStat> characterStats = new Dictionary<string, CharacterStat>();
    public Dictionary<string, SkillData> skilldatas = new Dictionary<string, SkillData>();

    [ContextMenu("Read CharacterStat CSV File")]
    private void ReadCSV()
    {
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
                stat.skillID = skillstring.Split(',');

                characterStats.Add(stat.characterID, stat);
                Debug.Log($"stat.name : {stat.name}");
                Debug.Log($"characterStats.Count : {characterStats.Count}");
            }
        }
        else if (readData == ReadData.skilldata)
        {
            Debug.Log("skilldata");
        }
    }
}
