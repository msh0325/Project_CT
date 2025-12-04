using System.Collections.Generic;
using UnityEngine;

public class TurnGameManager : MonoBehaviour
{
    [Header("Data")]
    public CSVReader csvReader;
    public TextAsset characterCSV;
    public TextAsset skillCSV;

    [Header("파티 구성")]
    public List<string> allyCharacterIDs;
    public List<string> enemyCharacterIDs;

    private List<BattleUnit> allies = new();
    private List<BattleUnit> enemies = new();
    private List<BattleUnit> turnOrder = new();
    
    private int currentTurnIndex = 0;
    private BattleState state = BattleState.Idle;

    void Start()
    {
        // 나중에 세부 구현할 때 데이터는 따로 매니저 빼서 관리하는게 좋을듯
        // 씬 전환되면서 데이터는 어케 옮김?
        // >> 캐릭터&스킬데이터는 DataManager(싱글톤 & dontdestroyonload), 유동적인 정보(즉, 캐릭터 성장, 편성 등)은 PlayerData(싱글톤 & dontdestroyonload)

        // characterData 불러오기
        csvReader.csvFile = characterCSV;
        csvReader.readData = ReadData.CharacterData;
        csvReader.ReadCSV();

        // skillData 불러오기
        csvReader.csvFile = skillCSV;
        csvReader.readData = ReadData.SkillData;
        csvReader.ReadCSV();
    }

    void Update()
    {
        
    }
}
