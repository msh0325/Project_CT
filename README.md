# Project_CT
## 해야 할것
 1. 방어 / 아이템 버튼 작동하게 만들기 
  > defend 작업 완료. 지금은 defendid를 하드코딩해둬서 받피감 50퍼 or 다음 피해 1회 무효화 둘 중 하나 작동. 나중에 방어 개성 이후에 characterstat에 id 넣을 듯
  > 아이템 버튼 작동 구현 완. 하지만, 아이템 버튼 누르면 itembox의 0번 아이템만 사용됨 + 갯수/쿨타임 적용 x. 이 부분 구현필요
 2. effect 한곳으로 통일하기 + 조건부 패시브 구현하기
  > effect랑 damage 주는걸 각각 effectpipeline / damagepipeline으로 정리 후 이걸 통해서만 effect/damage 줄수 있도록 수정.
  > 서포트 패시브도 effect에 편입함. effectdata의 duration이 -1이면(음수이면) 무한지속 체크하도록 수정.
  > 조건부 같은 경우는 각 순간마다 조건 체크 후 적용/미적용 하기
 3. 적 AI 만들기(최소 패턴 > 패턴/가중치 확장)
 4. 공격/방어 개성 기획하기

 Effectdata >> 지금 itemeffect 때문에 effect를 적용하는 함수를 override해서 itemeffect의 값으로 변경하게 만들었음. (기본은 5데미지지만, itemeffect에서 3데미지면 3으로 덮어씌우고 적용) 나중에 단순히 effect 값만 다르다던가, duration만 다르다던가 등 수치만 다른 effect가 필요하다면, effectdata를 하나 만드는게 아닌, override 해서 적용할 수 있도록 해야함. 아마 다른 애들은 거의 포함되지는 않겠지만, 다른 데이터도 이런 방식이 적용될 수 있다면, 그렇게 바꾸는게 데이터 양 적어져서 좋음.
## --------------------
전체적인 턴제 게임 구성이 완료되었으면 스토리 씬 구상 & 구현하기
    