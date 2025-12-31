using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Globalization;

public class CSVReader : MonoBehaviour
{
    [Header("CSV Data File")]
    public TextAsset characterCSV;
    public TextAsset skillCSV;
    public TextAsset effectCSV;
    public TextAsset stageCSV;
    public TextAsset waveCSV;
    public TextAsset supportCSV;
    public TextAsset itemCSV;
    public TextAsset passiveCSV;
    
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

            if(cols.Length < 10)
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
                critical = float.Parse(cols[8]),
                skillID = cols[9].Split('/',StringSplitOptions.RemoveEmptyEntries)
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

            if(cols.Length < 11)
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

            if(!Enum.TryParse<StatusType>(cols[10],out var statusType))
            {
                Debug.LogWarning($"statustype 파싱 실패 : {cols[10]}(line:{line})");
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
                maxDuration = int.Parse(cols[8]),
                statmul = float.Parse(cols[9]),
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
                Debug.LogWarning($"wave 컬럼 개수 부족 line : {line}");
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

            if(cols.Length < 6)
            {
                Debug.LogWarning($"support 컬럼 개수 부족(line : {line})");
                continue;
            }

            for(int j = 0; j < cols.Length; j++)
            {
                cols[j] = cols[j].Trim().Trim('"');
            }

            if(!Enum.TryParse<CastType>(cols[5], out var castType))
            {
                Debug.LogWarning($"casttype 파싱 실패 {cols[5]} (line:{line})");
                continue;
            }

            SupportData data = new SupportData
            {
                supportID = cols[0],
                name = cols[1],
                activeSkillID = cols[2],
                passiveSkillID = cols[3],
                attack = int.Parse(cols[4]),
                cast = castType
            };

            if(!saveFile.ContainsKey(data.supportID))
            {
                saveFile.Add(data.supportID,data);
            }
        }
    }

    public void ReadItemCSV(Dictionary<string, ItemData> saveFile)
    {
        if(itemCSV == null)
        {
            Debug.LogError("itemCSVFile이 비어있습니다");
            return;
        }

        string[] lines = itemCSV.text.Split('\n');
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

            if(cols.Length < 10)
            {
                Debug.LogWarning($"item 컬럼 개수 부족 line : {line}");
                continue;
            }

            for(int j = 0; j < cols.Length;j++)
            {
                cols[j] = cols[j].Trim().Trim('"');
            }

            if(!Enum.TryParse<ItemType>(cols[1],out var type))
            {
                Debug.LogWarning($"itemtype 파싱 실패 : {cols[1]} (line:{line})");
                continue;
            }

            EquipmentStats equipStat = new();

            if (!string.IsNullOrEmpty(cols[3]))
            {
                var stats = cols[3].Split(';',StringSplitOptions.RemoveEmptyEntries);
                foreach(var stat in stats)
                {
                    var token = stat.Trim();

                    int plusidx = token.IndexOf('+');
                    int multiidx = token.IndexOf('*');

                    bool isPlus = plusidx >= 0;
                    bool isMulti = multiidx >= 0;

                    if(isPlus == isMulti)
                    {
                        Debug.LogWarning($"equitstat 토큰 오류 : {token}");
                        continue;
                    }

                    int index = isPlus? plusidx:multiidx;
                    char op = isPlus? '+':'*';

                    string key = token[..index].Trim().ToLowerInvariant();
                    string valStr = token[(index+1)..].Trim();

                    if(op == '+')
                    {
                        if(!int.TryParse(valStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v))
                        {
                            Debug.LogWarning($"equipstats 파싱 실패 : int {token}");
                            continue;
                        }

                        ApplyFlat(equipStat,key,v);
                    }

                    if(op == '*')
                    {
                        if(!float.TryParse(valStr, NumberStyles.Float, CultureInfo.InvariantCulture, out var v))
                        {
                            Debug.LogWarning($"equipstats 파싱 실패 : float {token}");
                            continue;
                        }

                        ApplyMulti(equipStat,key,v);
                    }
                }
            }
            
            var targetType = TargetType.Self;
            if (!string.IsNullOrEmpty(cols[5]))
            {
                if(!Enum.TryParse<TargetType>(cols[5], out var t))
                {
                    Debug.LogWarning($"target 파싱 실패 : {cols[5]}(line:{line})");
                    continue;
                }
                targetType = t;
            }
            
            List<ItemEffect> effects = new();
            if (!string.IsNullOrEmpty(cols[6]))
            {
                var itemeffects = cols[6].Split(';',StringSplitOptions.RemoveEmptyEntries);
                foreach(var effect in itemeffects)
                {
                    var token = effect.Trim();

                    int first = token.IndexOf(':');
                    if(first < 0)
                    {
                        Debug.LogWarning($"itemeffect 형식이 잘못됨 {token}");
                        continue;
                    }
                    string id = token[..first].Trim().ToLowerInvariant();
                    
                    int second = token.IndexOf(':',first+1);
                    string v = "0";
                    string m = "0";
                    string d = "0";

                    if(second > 0)
                    {
                        int third = token.IndexOf(':',second+1);
                        if(third > 0)
                        {
                            v = token[(first+1)..second].Trim();
                            m = token[(second+1)..third].Trim();
                            d = token[(third+1)..].Trim();
                        }
                        else
                        {
                            v = token[(first+1)..second].Trim();
                            d = token[(second+1)..].Trim();
                        }
                    }
                    else
                    {
                        v = token[(first+1)..].Trim();
                    }

                    ItemEffect e = new ItemEffect
                    {
                        effectID = ItemEffectId(id),
                        value = int.Parse(v),
                        mul = float.Parse(m),
                        duration = int.Parse(d)
                    };

                    if (!effects.Any(f=>f.effectID == id))
                    {
                        effects.Add(e);
                    }
                }
            }

            int stack = 0;
            if(int.TryParse(cols[7],out var s))
            {
                stack = s;
            }

            int cool = 0;
            if(int.TryParse(cols[8],out var c))
            {
                cool = c;
            }

            ItemData data = new ItemData
            {
                itemID = cols[0],
                itemType = type,
                name = cols[2],
                isStackable = type == ItemType.Consumable,
                equipmentStats = equipStat,
                passiveID = cols[4],
                target = targetType,
                itemEffect = effects,
                maxStack = stack,
                cooltime = cool,
                iconKey = cols[9]
            };

            if (!saveFile.ContainsKey(data.itemID))
            {
                saveFile.Add(data.itemID,data);
            }
        }
    }

    private void ApplyFlat(EquipmentStats s, string key, int v)
    {
        switch (key)
        {
            case "hp" :
                s.flatHP += v;
                break;

            case "mp" :
                s.flatMP += v;
                break;

            case "atk" :
                s.flatAtk += v;
                break;

            case "def" : 
                s.flatDef += v;
                break;

            case "spd" : 
                s.flatSpeed += v;
                break;
            
            default :
                Debug.LogWarning($"잘못된 키 : {key}");
                break;
        }
    }

    private void ApplyMulti(EquipmentStats s, string key, float v)
    {
        switch (key)
        {
            case "hp" :
                s.mulHP *= v;
                break;
            
            case "mp" :
                s.mulMP *= v;
                break;
            
            case "atk" :
                s.mulAtk *= v;
                break;
            
            case "def" :
                s.mulDef *= v;
                break;
            
            case "cri" :
                s.mulCri *= v;
                break;
            
            default :
                Debug.LogWarning($"잘못된 키 : {key}");
                break;
        }
    }

    private string ItemEffectId(string id)
    {
        switch (id)
        {
            case "poison":
                return "EF_POISON";
            
            case "burn":
                return "EF_05";

            case "heal":
                return "EF_HEAL";
            
            case "clean":
                return "EF_CL";
            
            default:
                return "error";
        }
    }

    public void ReadPassiveCSV(Dictionary<string, Passive> saveFile)
    {
        if(passiveCSV == null)
        {
            Debug.LogError("passiveCSV가 비어있습니다.");
            return;
        }

        string []lines = passiveCSV.text.Split('\n');
        if(lines.Length <= 1)
        {
            Debug.LogWarning("csv 파일에 데이터가 없음");
            return;
        }

        for(int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();

            if(string.IsNullOrEmpty(line)) continue;

            string []cols = line.Split(',');

            if(cols.Length < 5)
            {
                Debug.LogWarning($"passive 컬럼 개수 부족 (line : {line})");
                continue;
            }

            for(int j = 0; j < cols.Length; j++)
            {
                cols[j] = cols[j].Trim().Trim('"');
            }

            if(!Enum.TryParse<BattleState>(cols[1],out var pstiming))
            {
                Debug.LogWarning($"battlestate 파싱 실패 : {cols[1]} (line:{line})");
                continue;
            }

            if(!Enum.TryParse<PassiveTrigger>(cols[2],out var pstrigger))
            {
                Debug.LogWarning($"passivetrigger 파싱 실패 : {cols[2]} (line:{line})");
                continue;
            }

            string condition = "empty";
            CompareOP op = CompareOP.None;
            float condition_value = -1;

            if (!string.IsNullOrEmpty(cols[3]))
            {
                var token = cols[3].Trim();
                int first = -1;
                string []ops = {">=","<=","==","!=",">","<"};
                string op_text = null;

                foreach(var o in ops)
                {
                    first = token.IndexOf(o, StringComparison.Ordinal);
                    if(first >= 0)
                    {
                        op_text = o;
                        break;
                    }
                }

                if(first < 0)
                {
                    Debug.LogWarning($"연산자 파싱 실패 line:{line}");
                    continue;
                }
                
                condition = token[..first].Trim().ToLowerInvariant();
                op = op_text switch
                {
                    ">=" => CompareOP.GE,
                    "<=" => CompareOP.LE,
                    "==" => CompareOP.EQ,
                    "!=" => CompareOP.NE,
                    ">" => CompareOP.GT,
                    "<" => CompareOP.LT,
                    _ => CompareOP.None
                };
                int op_len = op_text.Length;
                condition_value = float.Parse(token[(first+op_len+1)..].Trim());
            }

            StatusType stat = StatusType.None;
            float value = -1;

            if (!string.IsNullOrEmpty(cols[4]))
            {
                var token = cols[4].Trim();

                int index = token.IndexOf(':');

                string stat_text = token[..index].Trim().ToLowerInvariant();
                stat = StringToStat(stat_text);
                value = float.Parse(token[(index+1)..]);
            }

            Passive data = new Passive
            {
                passiveID = cols[0],
                timing = pstiming,
                trigger = pstrigger,
                condition = condition,
                op = op,
                condition_value = condition_value,
                stat = stat,
                value = value,
                applyNextRound = pstrigger == PassiveTrigger.AfterAction || pstrigger == PassiveTrigger.AfterDamageTaken? 1 : 0
            };

            if (!saveFile.ContainsKey(data.passiveID))
            {
                saveFile.Add(data.passiveID, data);
            }
        }
    }

    private StatusType StringToStat(string s)
    {
        return s switch
        {
            "atk" => StatusType.Attack,
            "def" => StatusType.Defense,
            "spd" => StatusType.Speed,
            _ => StatusType.None,
        };
    }
}
