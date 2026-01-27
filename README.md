# Project_CT
## 해야 할것
 1. 방어 / 아이템 버튼 작동하게 만들기 (clear)
  > defend 작업 완료. 지금은 defendid를 하드코딩해둬서 받피감 50퍼 or 다음 피해 1회 무효화 둘 중 하나 작동. 
  나중에 방어 개성 이후에 characterstat에 id 넣을 듯

  > 아이템 버튼 작동 구현 완.
  > 아이템 버튼 클릭하면 인벤토리 켯다 껏다 가능. 나중에 ui 개편 해야할것.
  > 아이템 쿨타임 표시 / 갯수 표시 완. 나중에 0개는 없앨지, 지금처럼 남겨둘지 다시 고민.

 2. effect 한곳으로 통일하기 + 조건부 패시브 구현하기
  > effect랑 damage 주는걸 각각 effectpipeline / damagepipeline으로 정리 후 이걸 통해서만 effect/damage 줄수 있도록 수정.
  > 서포트 패시브도 effect에 편입함. effectdata의 duration이 -1이면(음수이면) 무한지속 체크하도록 수정.

  > 조건부 같은 경우는 각 순간(패시브 데이터의 timing)마다 조건 체크 후 적용/미적용 하기

  > 원래 조건이 달성되는 순간부터 적용되게 하려고 했는데, 이것보단 그냥 timing 체크가 나을것 같음.

  > 라고 생각했지만 패시브 중에 특정 행동이 진행됐을 때 그 다음 라운드에 패시브가 적용되게 하려면 결국 trigger 가 필요해 trigger 만들어서 적용함. 그래서 passivedata나 다른 부분에 다음턴에 적용된다는 인자 필요.

  > passive에 trigger가 afteraction이나 afterdamagetaken일 때 applyNextround가 1이 됨. (다음 턴에 패시브 적용)
  아마 나중에 trigger가 더 세분화 될지도 모름.

 3. 적 AI 만들기(최소 패턴 > 패턴/가중치 확장)
  > EnemyAIController 에서 적 AI 관리. 
  > 당장은 랜덤한 타겟, Attack만 작동.
  > Scriptable로 ai 틀 만들고, 유닛마다 다른 우선순위를 가지게 함. (enemydata에서 ai 지정 필요)
  > 당장은 랜덤 / hp 낮은 애 / 공격력 젤 높은애 세개의 패턴 중 랜덤만 구현했음. 나중에 여러 패턴을 만들고 구현 필요.
 4. 공격/방어 개성 기획하기

 Effectdata >> 지금 itemeffect 때문에 effect를 적용하는 함수를 override해서 itemeffect의 값으로 변경하게 만들었음. (기본은 5데미지지만, itemeffect에서 3데미지면 3으로 덮어씌우고 적용) 나중에 단순히 effect 값만 다르다던가, duration만 다르다던가 등 수치만 다른 effect가 필요하다면, effectdata를 하나 만드는게 아닌, override 해서 적용할 수 있도록 해야함. 아마 다른 애들은 거의 포함되지는 않겠지만, 다른 데이터도 이런 방식이 적용될 수 있다면, 그렇게 바꾸는게 데이터 양 적어져서 좋음.
## --------------------
전체적인 턴제 게임 구성이 완료되었으면 스토리 씬 구상 & 구현하기
    