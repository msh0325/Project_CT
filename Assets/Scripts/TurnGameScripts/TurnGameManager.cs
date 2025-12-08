using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TurnGameManager : MonoBehaviour
{
    [Header("파티 구성")]
    public List<string> allyCharacterIDs;
    public List<string> enemyCharacterIDs;

    // 나중에 playerData 구현하면, 거기서 받아오기
    // 당장은 직접 입력으로 테스트만
    [Header("스테이지 구성")]
    public string stageID;

    private List<BattleUnit> allies = new();
    private List<BattleUnit> enemies = new();
    private List<BattleUnit> turnOrder = new();
    private List<WaveData> waves = new();

    private DataManager dataManager;
    
    private int nowWaveIndex = 0;
    private int currentTurnIndex = 0;
    private BattleState state = BattleState.Idle;
    private bool isBattleEnd = false;
    private System.Random rnd = new ();

    void Start()
    {
        // 나중에 세부 구현할 때 데이터는 따로 매니저 빼서 관리하는게 좋을듯
        // 씬 전환되면서 데이터는 어케 옮김?
        // >> 캐릭터&스킬데이터는 DataManager(싱글톤 & dontdestroyonload), 
        // 유동적인 정보(즉, 캐릭터 성장, 편성 등)은 PlayerData(싱글톤 & dontdestroyonload)

        if(dataManager == null && DataManager.instance != null)
        {
            dataManager = DataManager.instance;
        }
        StartCoroutine(BattleRoutine());
    }

    void Update()
    {
        // battleState를 외부 플래그용으로 사용하고(ex 플레이어 입력은 turnrun에서만 받게 하기, UI 특정 상황에서만 띄우기 등)
        // 전체적인 전투 흐름은 coroutine 이용하기?

        // 각 스테이지의 적 정보도 csv 파일로 정리하고 매니저로 관리하는게 좋을듯
    }

    // 전체적인 전투 흐름 코루틴
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
            
            for(int i = 0; i < turnOrder.Count; i++)
            {
                currentTurnIndex = i;
                BattleUnit nowUnit = turnOrder[currentTurnIndex];
                // 3. turnOrder 순으로 캐릭터 턴 시작 (state = TurnStart)
                state = BattleState.TurnStart;
                // 턴 진행중 죽었으면 다음 순서
                if(nowUnit.isDead) continue;

                // 4. 캐릭터 행동 대기. 플레이어의 주/부 행동 입력 or 적의 AI 입력 (state = RunTurn)
                Debug.Log($"now {nowUnit.name}'s turn. hp : {nowUnit.currentHP} / mp : {nowUnit.currentMP}");
                state = BattleState.RunTurn;
                
                BattleUnit target = null;

                // 임시로 각 팀의 첫번째 유닛을 공격하게 만듦.
                if(nowUnit.team == TeamType.Ally)
                {
                    target = enemies.FirstOrDefault(u => !u.isDead);
                }
                else
                {
                    target = allies.FirstOrDefault(u =>!u.isDead);
                }

                if(target != null)
                {
                    nowUnit.TestAttack(target,rnd);                    
                }
                
                yield return new WaitForSeconds(2f);
                //yield return new WaitUntil(characterTurnEnd); << 이런느낌?

                // 5. 캐릭터 행동 종료. turnOrder에 남은 캐릭터 있으면 3번부터 시작 (state = TurnEnd)
                state = BattleState.TurnEnd;
            }
            // 6. 모든 캐릭터가 행동 종료했으면 라운드 종료 (state = RoundEnd)
            state = BattleState.RoundEnd;
            // 전투 종료 체크하기 & 웨이브 체크하기
            CheckBattleOver();
        }

        // 전투 종료.
        state = BattleState.Idle;
    }

    private void SetUpBattleUnits()
    {
        // 현재 참여중인 캐릭터 체크하기
        // 아군은 플레이어가 편성한 로스터 확인하기
        // 적군은 datamanager에 있는 stagedata & wavedata로 확인하기
        
        // waves에 선택된 stage wave 정보 넣기
        if(dataManager.waveDatas.TryGetValue(stageID,out var stageWaves))
        {
            waves.AddRange(stageWaves);
        }
        // waves에서 이번 전투의 enemyID 받기
        enemyCharacterIDs.AddRange(waves[nowWaveIndex].enemyID);

        // 당장은 직접 입력해서 팀 데이터 읽기
        foreach(string ally in allyCharacterIDs)
        {
            if (dataManager.characterStats.ContainsKey(ally))
            {
                BattleUnit unit = new BattleUnit(dataManager.characterStats[ally],TeamType.Ally);
                allies.Add(unit);
                turnOrder.Add(unit);

                unit.InitSkills(dataManager.skillDatas);
                
                Debug.Log($"add new {unit.team} {dataManager.characterStats[ally].name}");
            }
        }
        
        foreach(string enemy in enemyCharacterIDs)
        {
            if (dataManager.characterStats.ContainsKey(enemy))
            {
                BattleUnit unit = new(dataManager.characterStats[enemy],TeamType.Enemy);
                enemies.Add(unit);
                turnOrder.Add(unit);

                unit.InitSkills(dataManager.skillDatas);

                Debug.Log($"add new {unit.team} {dataManager.characterStats[enemy].name}");
            }
        }
    }

    private void SetUpWaveBattleUnits(int waveIndex)
    {
        turnOrder.Clear();
        enemies.Clear();
        enemyCharacterIDs.Clear();

        foreach(var ally in allies)
        {
            if (!ally.isDead)
            {
                turnOrder.Add(ally);
            }
        }
        
        enemyCharacterIDs.AddRange(waves[waveIndex].enemyID);
        
        foreach(var enemy in enemyCharacterIDs)
        {
            if (dataManager.characterStats.ContainsKey(enemy))
            {
                BattleUnit unit = new(dataManager.characterStats[enemy],TeamType.Enemy);
                enemies.Add(unit);
                turnOrder.Add(unit);

                unit.InitSkills(dataManager.skillDatas);

                Debug.Log($"new wave{nowWaveIndex} add new {unit.team} {dataManager.characterStats[enemy].name}");
            }
        }
    }

    private void RollUnitSpeed()
    {
        // 죽은 캐릭터 turnOrder에서 제외
        turnOrder = allies.Concat(enemies).Where(u=>!u.isDead).ToList();

        // 현재 참여중인 캐릭터 속도 굴리기
        foreach(BattleUnit unit in turnOrder)
        {
            unit.RollSpeed(rnd);
            Debug.Log($"{unit.name}'s speed : {unit.currentSpeed}");
        }

        // 속도순으로 내림차순. 그러나 속도가 같은 경우 이전 순서대로 정렬됨
        turnOrder.Sort((a, b) =>
        {
            int speedCompare = b.currentSpeed.CompareTo(a.currentSpeed);
            if(speedCompare != 0) return speedCompare;
            
            // 그래서 아군이 먼저 정렬되도록 추가 코드 (ally=0 / enemy=1)
            int teamCompare = a.team.CompareTo(b.team);
            if(teamCompare != 0) return teamCompare;

            // 나중에 팀이 같을 시, 순서 정하는 코드도 필요할듯.
            // 팀이 같으면, 앞 열부터 시작
            int rowCompare = a.row.CompareTo(b.row);
            if(rowCompare != 0) return rowCompare;

            return 0;
        });
    }

    private void CheckBattleOver()
    {
        // 전투 종료 체크 >> 모든 아군 사망 or 모든 적 사망
        bool alliesAllDead = allies.All(u => u.isDead);
        bool enemiesAllDead = enemies.All(u => u.isDead);

        if (alliesAllDead)
        {
            Debug.Log("defeat");
            isBattleEnd = true;
            return;
        }

        if (enemiesAllDead)
        {
            if (HasNextWave())
            {
                nowWaveIndex++;
                Debug.Log("next wave");
                SetUpWaveBattleUnits(nowWaveIndex);
                return;
            }
            else
            {
                Debug.Log("victory");
                isBattleEnd = true;
                return;
            }
        }
    }

    private bool HasNextWave()
    {
        if(!dataManager.waveDatas.TryGetValue(stageID,out var wavedata))
        {
            return false;
        }
        return nowWaveIndex + 1 < waves.Count;
    }
}

