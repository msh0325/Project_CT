
public enum BattleState
{
    Idle, // 초기 상태
    SetUp, // 캐릭터 받아오기
    RoundStart, // 라운드 시작, 속도 굴림 후 턴 순서 결정, 라운드 시작 상태이상 체크
    TurnStart, // 캐릭터 턴 시작, 턴 시작 상태이상 체크
    RunTurn, // 실제 행동 (스킬 선택 / AI / 연출)
    TurnEnd, // 캐릭터 턴 종료, 턴 종료 상태이상 체크
    RoundEnd // 라운드 종료, 라운드 종료 상태이상 체크, 게임 승리/패배 조건 체크
}