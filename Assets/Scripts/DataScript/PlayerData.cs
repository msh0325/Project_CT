using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class PlayerData : MonoBehaviour
{
    public static PlayerData instance;
    public DataManager dataManager;

    // 로스터 : 플레이어가 현재 가지고 있는 캐릭터 정보
    public List<string> ownedCharacters = new();
    public List<PlayerCharacterStat> roster = new();
    public Dictionary<string, PlayerCharacterStat> rosterMap = new();
    // 편성한 파티 : 플레이어가 선택한 전투 유닛 3명 + 서포트 유닛 1명 (이보다 적을 수 있음)
    public List<PartyMemberSetting> selectedParty = new();
    public Dictionary<string, PartyMemberSetting> selectedPartyMap = new();
    // 서포트 로스터 : 플레이어가 현재 가지고 있는 서포트 캐릭터 정보
    public List<string> ownedSupports = new();
    public List<SupportData> supportRoster = new();
    public SupportData selectedSupport = new();
    public string nowSelectStageID; // 임시 변수
    // 진행 상황 : 클리어현황, 특정 기능 해금
    // 인벤토리 : 메인/서브퀘 깨면서 얻은 아이템들. 퀘스트 아이템도 포함
    public List<string> itemList = new();
    public List<ItemData> itemBox = new();
    public Dictionary<string, ItemData> itemBoxMap = new();
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
        SetDataMap();
    }

    void Start()
    {
        dataManager = DataManager.instance;
        
        SetRoster();
        SetSupportRoster();

        // item 잘 파서됐는지 확인용 코드
        foreach(var id in itemList)
        {
            if(dataManager.itemData.TryGetValue(id, out var item))
            {
                itemBox.Add(item);
                itemBoxMap.Add(id,item);
            }
        }
    }

    private void SetRoster()
    {
        roster.Clear();
        rosterMap.Clear();

        foreach(var id in ownedCharacters)
        {
            if (!dataManager.characterStats.TryGetValue(id,out var stat))
            {
                Debug.LogWarning($"{id}가 characterstat에 없음");
                continue;
            }

            var member = new PlayerCharacterStat
            {
                characterID = id,
                isSelectable = true,
                learnedSkillID = stat.skillID.ToList()
            };

            roster.Add(member);
            rosterMap[id] = member;
        }
    }

    private void SetSupportRoster()
    {
        supportRoster.Clear();

        foreach(var id in ownedSupports)
        {
            if (!dataManager.supportData.TryGetValue(id,out var support))
            {
                Debug.LogWarning($"{id}가 supportdata에 없음");
                continue;
            }

            supportRoster.Add(support);
        }
    }

    private void SetDataMap()
    {
        rosterMap.Clear();
        foreach(var c in roster)
        {
            if(!rosterMap.ContainsKey(c.characterID))
            rosterMap.Add(c.characterID, c);
        }

        selectedPartyMap.Clear();
        foreach(var c in selectedParty)
        {
            if(!selectedPartyMap.ContainsKey(c.characterID))
            selectedPartyMap.Add(c.characterID,c);
        }
    }
}