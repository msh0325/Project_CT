using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TurnGameManager : MonoBehaviour
{
    public static TurnGameManager instance;
    public event Action<BattleUnit> OnPlayerTurnStart;
    
    [Header("테스트용 인스펙터")]
    public BattleUI testPrefab;
    public Transform[] allySlots;
    public Transform[] enemySlots;
    public BattleUIManager uiManager;
    public bool isPlayerChecked = false;
    public BattleUnit currentUnit => turnOrder[currentTurnIndex];
    public bool IsWaitingPlayerInput => isPlayerChecked;

    private List<BattleUI> uis = new();
    private GameObject[] enemyPrefabs = new GameObject[3];
    private BattleCommandType command;
    private SkillData selectSkill;
    private ItemData selectItem;
    private BattleUnit selectedTarget;
    private string stageID;
    private List<BattleUnit> turnOrder = new();
    private List<WaveData> waves = new();
    [SerializeField] private SupportUnit support = new();

    private DataManager dataManager;
    private PlayerData pcDataManager;
    public PassiveSystem passiveSystem;
    public BattleContext battleContext;
    
    private int currentTurnIndex = 0;
    public BattleState state = BattleState.Idle;
    private bool isBattleEnd = false;
    private bool isWaveEnd = false;
    private System.Random rnd = new ();

    public EnemyAIController enemyAI = new EnemyAIController();

    public enum BattleCommandType
    {
        Attack,
        Skill,
        Defend,
        Item,
        Support
    }

    void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this.gameObject);
            return;
        }
    }

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

        if(pcDataManager == null && PlayerData.instance != null)
        {
            pcDataManager = PlayerData.instance;
        }
        stageID = pcDataManager.nowSelectStageID;

        StartCoroutine(BattleRoutine());
    }

    void Update()
    {
        // battleState를 외부 플래그용으로 사용하고(ex 플레이어 입력은 turnrun에서만 받게 하기, UI 특정 상황에서만 띄우기 등)
        // 전체적인 전투 흐름은 coroutine 이용하기?
        if(state == BattleState.Idle && Input.GetKeyDown(KeyCode.F))
        {
            SceneManager.LoadScene("StoryScene");
        }
    }

    // 전체적인 전투 흐름 코루틴
    IEnumerator BattleRoutine()
    {
        // 1. 현재 참여중인 캐릭터 체크 (state = SetUp)
        state = BattleState.SetUp;
        SetUpBattleUnits();
        uiManager.skillUIPannel.Init();

        // 2. 전투 종료(isBattleEnd) 가 아니면 전투 시작
        while(!isBattleEnd)
        {
            // 라운드 들어가기 전, 게임 종료 체크
            if(CheckBattleOver())
            {
                state = BattleState.Idle;
                yield break;
            }
            // 전투 참여 캐릭터 속도 굴림 후 속도 순 정렬하기
            state = BattleState.RoundStart;
            bool breakRound = false;
            foreach(var u in turnOrder)
            {
                u.CheckEffect(state);
            }
            if(support != null && support.supportPassive != null)
            {
                //foreach(var u in allies.Where(u => !u.isDead))
                foreach(var u in battleContext.allies.Where(u => !u.isDead))
                {
                    support.passiveRuntime.UpdatePassive(u,state);
                    foreach(var pr in u.passives)
                    {
                        pr.TryApplyPendingOnRoundStart(u, battleContext.currentRound);
                    }
                }
            }

            RollUnitSpeed();
            support?.TickCoolDown();
            foreach(var item in pcDataManager.itemBoxMap)
            {
                item.Value.TickCoolDown();
            }
            
            for(int i = 0; i < turnOrder.Count; i++)
            {
                currentTurnIndex = i;
                BattleUnit nowUnit = turnOrder[currentTurnIndex];

                // 턴 시작 전에 게임 종료 체크
                if(CheckBattleOver())
                {
                    state = BattleState.Idle;
                    yield break;
                }
                // 3. turnOrder 순으로 캐릭터 턴 시작 (state = TurnStart)
                state = BattleState.TurnStart;
                if(nowUnit.team == TeamType.Ally && support != null) support.passiveRuntime?.UpdatePassive(nowUnit,state);
                // 턴 진행중 죽었으면 다음 순서
                if(nowUnit.isDead) continue;                
                // 4. 캐릭터 행동 대기. 플레이어의 주/부 행동 입력 or 적의 AI 입력 (state = RunTurn)
                Debug.Log($"now {nowUnit.name}'s turn.");
                nowUnit.CheckEffect(state);
                state = BattleState.RunTurn;
                
                nowUnit.TickCoolDown();

                BattleUnit target = null;

                if(nowUnit.team == TeamType.Ally)
                {
                    OnPlayerTurnStart?.Invoke(nowUnit);
                    bool isActionDone = false;
                    nowUnit.OnTurnStart();
                    while (!isActionDone)
                    {
                        if(nowUnit.isDead) break;
                        uiManager.skillUIPannel.CheckCanUseSkill(nowUnit);
                        uiManager.CheckCanUseSupport(support,nowUnit);
                        isPlayerChecked = true;

                        yield return new WaitUntil(()=> isPlayerChecked == false);

                        target = selectedTarget;
                        isActionDone = ExecutePlayerCommand(nowUnit,target,command);
                        foreach(var ui in uis) ui.Refresh();

                        if (CheckBattleOver())
                        {
                            state = BattleState.Idle;
                            yield break;
                        }

                        if (isWaveEnd)
                        {
                            isWaveEnd = false;
                            breakRound = true;
                            break;
                        }
                        yield return null;
                    }
                }
                else
                {
                    // EnemyAIController에 따라 타겟과 액션 결정
                    // 지금은 랜덤 타겟에 attack 만 작동
                    bool isActionDone = false;
                    nowUnit.OnTurnStart();
                    while(!isActionDone)
                    {
                        if(nowUnit.isDead) break;
                        AIAction action = nowUnit.profile.Decide(nowUnit, battleContext);
                        isActionDone = ExecuteAIAction(nowUnit, action);
                        foreach(var ui in uis) ui.Refresh();

                        if (CheckBattleOver())
                        {
                            state = BattleState.Idle;
                            yield break;
                        }

                        if (isWaveEnd)
                        {
                            isWaveEnd = false;
                            breakRound = true;
                            break;
                        }
                        yield return null;
                    }
                }
                uiManager.HidePlayerUI();    

                // 5. 캐릭터 행동 종료. turnOrder에 남은 캐릭터 있으면 3번부터 시작 (state = TurnEnd)
                state = BattleState.TurnEnd;
                nowUnit.CheckEffect(state);

                // 행동 직후 게임 종료 체크
                if(CheckBattleOver())
                {
                    state = BattleState.Idle;
                    yield break;
                }
                // ui 리프레시
                foreach(var ui in uis)
                {
                    ui.Refresh();
                }
                
                yield return new WaitUntil(()=>Input.GetKeyDown(KeyCode.Space));
                
                if(breakRound) break;
            }
            // 6. 모든 캐릭터가 행동 종료했으면 라운드 종료 (state = RoundEnd)
            state = BattleState.RoundEnd;
            battleContext.NextRound();
            foreach(var u in turnOrder)
            {
                u.CheckEffect(state);
            }
            // 전투 종료 체크하기 & 웨이브 체크하기. 
            // 나중에 라운드 종료 상태이상 적용하면 그거로 죽고 끝날 수 있으니 나중에 다시 주석 풀기
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
        if(!dataManager.waveDatas.TryGetValue(stageID,out var stageWaves))
        {
            Debug.LogError($"waveDatas에 {stageID} 가 없음");
            return;
        }
        waves.AddRange(stageWaves);

        foreach(var ally in pcDataManager.selectedParty)
        {
            if(!dataManager.characterStats.TryGetValue(ally.characterID,out var baseStat))
            {
                Debug.LogWarning($"캐릭터 id : {ally.characterID}가 characterStat에 없음");
                continue;
            }
            pcDataManager.rosterMap.TryGetValue(ally.characterID, out var bonus);
            pcDataManager.selectedPartyMap.TryGetValue(ally.characterID,out var party);

            BattleUnit unit = new(baseStat, TeamType.Ally, ally.row, bonus, party)
            {
                partyChar = ally
            };
            //allies.Add(unit);
            battleContext.allies.Add(unit);
            turnOrder.Add(unit);

            unit.InitSkills(dataManager.skillDatas);

            // 임시 프리팹 생성
            var view = Instantiate(testPrefab,allySlots[(int)unit.row].position,Quaternion.identity);
            view.Init(unit);
            uis.Add(view);
            uiManager.RegisterBattleUI(view);
        }

        SpawnEnemyUnit(battleContext.currentWave);
        DamagePipeline.Init(passiveSystem, battleContext);

        // 서포트 캐릭터 불러오기
        support = SupportUnit.TryCreate(PlayerData.instance,dataManager);
        //support?.ApplySupportPassive(allies);
        support?.ApplySupportPassive(battleContext.allies);
    }

    private void SetUpWaveBattleUnits(int waveIndex)
    {
        turnOrder.Clear();
        //enemies.Clear();
        battleContext.enemies.Clear();

        //foreach(var ally in allies)
        foreach(var ally in battleContext.allies)
        {
            if (!ally.isDead)
            {
                turnOrder.Add(ally);
            }
        }

        SpawnEnemyUnit(waveIndex);
    }

    private void RollUnitSpeed()
    {
        // 죽은 캐릭터 turnOrder에서 제외
        //turnOrder = allies.Concat(enemies).Where(u=>!u.isDead).ToList();
        turnOrder = battleContext.allies.Concat(battleContext.enemies).Where(u=>!u.isDead).ToList();

        // 현재 참여중인 캐릭터 속도 굴리기
        foreach(BattleUnit unit in turnOrder)
        {
            unit.RollSpeed(rnd);
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

    private bool CheckBattleOver()
    {
        // 전투 종료 체크 >> 모든 아군 사망 or 모든 적 사망
        //bool alliesAllDead = allies.All(u => u.isDead);
        bool alliesAllDead = battleContext.allies.All(u=>u.isDead);
        //bool enemiesAllDead = enemies.All(u => u.isDead);
        bool enemiesAllDead = battleContext.enemies.All(u=>u.isDead);

        if (alliesAllDead)
        {
            Debug.Log("defeat");
            isBattleEnd = true;
            return true;
        }

        if (enemiesAllDead)
        {
            if (HasNextWave())
            {
                battleContext.NextWave();  
                battleContext.currentRound = 0;
                isWaveEnd = true;
                Debug.Log("next wave");
                uiManager.ForceExitSelectMode();
                for(int i = 0; i < enemyPrefabs.Length; i++)
                {
                    Destroy(enemyPrefabs[i]);
                }
                SetUpWaveBattleUnits(battleContext.currentWave);
                return false;
            }
            else
            {
                Debug.Log("victory");
                isBattleEnd = true;
                return true;
            }
        }
        return false;
    }

    private bool HasNextWave()
    {
        if (!dataManager.waveDatas.ContainsKey(stageID))
        {
            return false;
        }
        return battleContext.currentWave + 1 < waves.Count;
    }

    // 플레이어는 한번 소환하면 끝이지만, 적은 wave마다 소환이 필요해 따로 빼둠
    private void SpawnEnemyUnit(int waveIndex)
    {
        var wave = waves[waveIndex];

        int rowCount = 0;
        for(int i = 0; i < wave.enemyID.Length; i++)
        {
            string enemy = wave.enemyID[i];
            
            if(!dataManager.characterStats.TryGetValue(enemy,out var baseStat))
            {
                Debug.LogWarning($"캐릭터 id : {enemy}가 characterStat에 없음");
                continue;
            }

            int count = wave.enemyCount[i];

            for(int c = 0; c < count; c++)
            {
                BattleUnit unit = new(baseStat,TeamType.Enemy, wave.enemyRow[rowCount]);
                //enemies.Add(unit);
                battleContext.enemies.Add(unit);
                turnOrder.Add(unit);
                unit.InitSkills(dataManager.skillDatas);

                var view = Instantiate(testPrefab,enemySlots[(int)unit.row].position,Quaternion.identity);
                view.Init(unit);
                uis.Add(view);
                uiManager.RegisterBattleUI(view);
                enemyPrefabs[rowCount] = view.gameObject;
                rowCount++;
            }
        }
    }

    // 플레이어의 행동 실행 함수
    private bool ExecutePlayerCommand(BattleUnit unit, BattleUnit target, BattleCommandType cmd)
    {
        // 주 행동 모두 소모하면 즉시 턴 종료.
        // 턴을 안 까먹는 행동(부 행동)은 false, 턴을 까먹는 행동(주 행동)은 true 리턴
        switch(cmd)
        {
            case BattleCommandType.Attack:
                if(!unit.CanUseMainAction()) return true;
                if(target != null) unit.Attack(target);
                unit.UseMainAction();
                passiveSystem.NotifyTirgger(unit,PassiveTrigger.AfterAction, battleContext.currentRound);
                return !unit.CanUseMainAction();
            
            case BattleCommandType.Skill:
                if(!unit.CanUseMainAction()) return true;
                if(!unit.CanUseSkill(selectSkill.skillID)) return false;

                unit.ConsumeSkillCost(selectSkill);
                float power = unit.CalcSkillRealDamage(selectSkill);

                switch (selectSkill.targetType)
                {
                    case TargetType.AllySingle:
                    case TargetType.EnemySingle:
                    case TargetType.Self:
                        unit.TakeDamage(target,selectSkill,power);
                        break;
                    
                    case TargetType.AllyAll:
                        {
                            //var targets = allies.Where(u=>!u.isDead);
                            var targets = battleContext.allies.Where(u=>!u.isDead);
                            foreach(var t in targets)
                            {
                                unit.TakeDamage(t,selectSkill,power);
                            }
                            break;
                        }
                    
                    case TargetType.EnemyAll:
                        {
                            //var targets = enemies.Where(u=>!u.isDead);
                            var targets = battleContext.enemies.Where(u=>!u.isDead);
                            foreach(var t in targets)
                            {
                                unit.TakeDamage(t,selectSkill,power);
                            }
                            break;
                        }
                }
                unit.UseMainAction();
                Debug.Log("use skill");
                return !unit.CanUseMainAction();

            case BattleCommandType.Defend:
                if(!unit.CanUseMainAction()) return true;
                
                EffectPipeline.ApplyEffectPacket(unit, new EffectEvent
                {
                    baseData = unit.defend,
                    source = unit
                });
                unit.UseMainAction();
                Debug.Log("use defend");
                return true;

            case BattleCommandType.Item:
                if(!unit.CanUseSubAction()) return false;
                if(!pcDataManager.itemBoxMap.TryGetValue(selectItem.itemID, out var item))
                {
                    Debug.LogWarning($"itemboxmap에서 {selectItem.itemID}를 찾을 수 없음");
                    return false;
                }
                if(!item.CanUseItem()) return false;

                foreach(var itemEffect in selectItem.itemEffect)
                {
                    if(dataManager.effectDatas.TryGetValue(itemEffect.effectID, out var e))
                    {
                        switch (selectItem.target)
                        {
                            case TargetType.EnemySingle:
                            case TargetType.AllySingle:
                            case TargetType.Self:
                                EffectPipeline.ApplyEffectPacket(target, new EffectEvent
                                {
                                    baseData = e,
                                    value = itemEffect.value,
                                    mul = itemEffect.mul,
                                    duration = itemEffect.duration,
                                    source = unit
                                });
                                break;

                            case TargetType.EnemyAll:
                                {
                                    //var targets = enemies.Where(u => !u.isDead);
                                    var targets = battleContext.enemies.Where(u=>!u.isDead);
                                    foreach(var t in targets)
                                    {
                                        EffectPipeline.ApplyEffectPacket(t, new EffectEvent
                                        {
                                            baseData = e,
                                            value = itemEffect.value,
                                            mul = itemEffect.mul,
                                            duration = itemEffect.duration,
                                            source = unit
                                        });
                                    }
                                    break;
                                }
                            
                            case TargetType.AllyAll:
                                {
                                    //var targets = allies.Where(u => !u.isDead);
                                    var targets = battleContext.allies.Where(u=>!u.isDead);
                                    foreach(var t in targets)
                                    {
                                        EffectPipeline.ApplyEffectPacket(t,new EffectEvent
                                        {
                                            baseData = e,
                                            value = itemEffect.value,
                                            mul = itemEffect.mul,
                                            duration = itemEffect.duration,
                                            source = unit
                                        });
                                    }
                                    break;
                                }
                        }
                    }
                }
                
                item.UseItem();
                uiManager.itemPannel.GetComponent<ItemInventoryPannel>().Refresh();
                uiManager.itemPannel.ClearSelectSlot();
                
                Debug.Log("use item");
                unit.UseSubAction();
                return false;

            case BattleCommandType.Support:
                if(!unit.CanUseSubAction()) return false;
                if(support == null) return false;
                if(!support.CanUseSupport()) return false;
                Debug.Log("use supportSkill");
                int subDmg = 0;
                switch (selectSkill.targetType)
                {
                    case TargetType.AllySingle:
                    case TargetType.EnemySingle:
                    case TargetType.Self:
                        subDmg = support.CalcDamage(target);
                        DamagePipeline.Apply(new DamageEvent
                        {
                            target = target,
                            amount = subDmg,
                            kind = DamageKind.Direct,
                            allowDamageReduction = true
                        });
                        break;
                    
                    case TargetType.AllyAll:
                        //var targets = allies.Where(u=>!u.isDead);
                        var targets = battleContext.allies.Where(u=>!u.isDead);
                        foreach(var t in targets)
                        {
                            subDmg = support.CalcDamage(t);
                            DamagePipeline.Apply(new DamageEvent
                            {
                                target = t,
                                amount = subDmg,
                                kind = DamageKind.Direct,
                                allowDamageReduction = true
                            });
                        }
                        break;
                    
                    case TargetType.EnemyAll:
                        //var tar = enemies.Where(u=>!u.isDead);
                        var tar = battleContext.enemies.Where(u=>!u.isDead);
                        foreach(var t in tar)
                        {
                            subDmg = support.CalcDamage(t);
                            DamagePipeline.Apply(new DamageEvent
                            {
                                target = t,
                                amount = subDmg,
                                kind = DamageKind.Direct,
                                allowDamageReduction = true
                            });
                        }
                        break;
                }

                support?.StartCooldown();
                unit.UseSubAction();
                return false;
            
            default:
                return false;
        }
    }

    public bool ExecuteAIAction(BattleUnit unit, AIAction action)
    {
        if(action.target == null) return false;

        var target = action.target;

        if (string.IsNullOrEmpty(action.skillID))
        {
            unit.Attack(target);
            unit.UseMainAction();
            Debug.Log($"{unit.name}이 {target.name}을 향해 Attack 공격");
            return true;
        }
        else
        {
            if(!dataManager.skillDatas.TryGetValue(action.skillID, out var skill))
            {
                Debug.LogWarning($"skilldata에 {action.skillID} 가 없음");
                return false;
            }

            unit.ConsumeSkillCost(skill);
            float power = unit.CalcSkillRealDamage(skill);

            switch (skill.targetType)
            {
                case TargetType.AllySingle:
                case TargetType.EnemySingle:
                case TargetType.Self:
                    unit.TakeDamage(target,skill,power);
                    Debug.Log($"{unit.name}이 {target.name}을 향해 skill 공격");
                    break;
                
                case TargetType.AllyAll:
                    {
                        var targets = battleContext.enemies.Where(u=>!u.isDead);
                        foreach(var t in targets)
                        {
                            unit.TakeDamage(t,skill,power);
                        }
                        break;
                    }
                
                case TargetType.EnemyAll:
                    {
                        var targets = battleContext.allies.Where(u=>!u.isDead);
                        foreach(var t in targets)
                        {
                            unit.TakeDamage(t,skill,power);
                        }
                        break;
                    }
            }
            unit.UseMainAction();
            return true;
        }
    }

    public void OnPlayerSelectCommand(BattleCommandType cmd, SkillData skill = null, ItemData item = null)
    {
        if(!isPlayerChecked) return;

        switch (cmd)
        {
            case BattleCommandType.Attack:
                {
                    var candidates = battleContext.AttackRangeTargets(currentUnit);
                    uiManager.EnterTargetSelectMode(candidates, (target) =>
                    {
                        command = cmd;
                        selectedTarget = target;
                        selectSkill = skill;
                        isPlayerChecked = false;
                    });
                }
                break;
                
            case BattleCommandType.Skill:
                {
                    var candidates = battleContext.SkillRangeTargets(currentUnit,skill);
                    uiManager.EnterTargetSelectMode(candidates, (target) =>
                    {
                        command = cmd;
                        selectedTarget = target;
                        selectSkill = skill;
                        isPlayerChecked = false;
                    });
                }
                break;
            
            case BattleCommandType.Defend:
                {
                    var candidates = battleContext.DefenseRangeTarget(currentUnit);
                    uiManager.EnterTargetSelectMode(candidates, (target) =>
                    {
                        command = cmd;
                        selectedTarget = target;
                        selectSkill = skill;
                        isPlayerChecked = false;
                    });
                }
                break;
            
            case BattleCommandType.Item:
                {
                    var candidates = battleContext.ItemRangeTarget(currentUnit, item);
                    uiManager.EnterTargetSelectMode(candidates, (target) =>
                    {
                        command = cmd;
                        selectedTarget = target;
                        selectSkill = skill;
                        selectItem = item;
                        isPlayerChecked = false;
                    });
                }
                break;
            
            case BattleCommandType.Support:
                {
                    if(support == null) return;
                    var candidates = battleContext.SkillRangeTargets(currentUnit,support.supportSkill);
                    uiManager.EnterTargetSelectMode(candidates, (target) =>
                    {
                        command = cmd;
                        selectedTarget = target;
                        selectSkill = support.supportSkill;
                        isPlayerChecked = false;
                    });
                }
                break;
        }
    }

    public void DeleteUIList(BattleUI ui)
    {
        uis.Remove(ui);
    }
}

