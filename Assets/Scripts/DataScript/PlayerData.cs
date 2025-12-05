using UnityEngine;
using System.Collections.Generic;

public class PlayerData : MonoBehaviour
{
    public static PlayerData instance;
    // 로스터 : 플레이어가 현재 가지고 있는 캐릭터 정보
    public List<PlayerCharacterStat> roster = new();
    // 편성한 파티 : 플레이어가 선택한 전투 유닛 3명 + 서포트 유닛 1명 (이보다 적을 수 있음)
    public List<PartyMemberSetting> selectedParty = new();
    public string nowSelectStageID;
    // 진행 상황 : 클리어현황, 특정 기능 해금
    // 인벤토리 : 메인/서브퀘 깨면서 얻은 아이템들. 퀘스트 아이템도 포함
    // 이벤트 플래그 : 튜토리얼 클리어, NPC 만남 등

    void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}