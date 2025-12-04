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
    private bool isBattleEnd = false;

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
        // battleState를 외부 플래그용으로 사용하고(ex 플레이어 입력은 turnrun에서만 받게 하기, UI 특정 상황에서만 띄우기 등)
        // 전체적인 전투 흐름은 coroutine 이용하기?
    }

    IEnumerator BattleRoutine()
    {
        // 1. 현재 참여중인 캐릭터 체크 (state = SetUp)
        state = BattleState.SetUp;
        SetUpBattleUnits();

        // 2. 전투 종료(isBattleEnd) 가 아니면 전투 시작
        while(!isBattleEnd)
        {
            // 전투 참여 캐릭터 속도 굴림 후 속도 순 정렬하기
            state = BattleState.RoundStart;
            RollUnitSpeed();
            
            for(currentTurnIndex = 0;currentTurnIndex<turnOrder.count;currentTurnIndex++)
            {
                // 3. turnOrder 순으로 캐릭터 턴 시작 (state = TurnStart)
                state = BattleState.TurnStart;

                // 4. 캐릭터 행동 대기. 플레이어의 주/부 행동 입력 or 적의 AI 입력 (state = RunTurn)
                state = BattleState.RunTurn;

                // 5. 캐릭터 행동 종료. turnOrder에 남은 캐릭터 있으면 3번부터 시작 (state = TurnEnd)
                state = BattleState.TurnEnd;
            }  
            // 6. 모든 캐릭터가 행동 종료했으면 라운드 종료 (state = RoundEnd)
            state = BattleState.RoundEnd;
            // 전투 종료 체크하기 (모든 아군이 죽었는지, 모든 적이 죽었는지 체크)
        }
    }

    private void SetUpBattleUnits()
    {
        // 현재 참여중인 캐릭터 체크하기
    }

    private void RollUnitSpeed()
    {
        // 현재 참여중인 캐릭터 속도 굴리기
    }
}

