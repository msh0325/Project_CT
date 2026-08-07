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
    public List<ItemStack> itemBox = new();
    public Dictionary<string, ItemStack> itemBoxMap = new();
    static Dictionary<string, Sprite> _iconCache = new();
    // 이벤트 플래그 : 튜토리얼 클리어, NPC 만남 등

    // 현재 플레이어가 가지고 있는 돈
    public int pcMoney = 0;

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
        _iconCache["default"] = Resources.Load<Sprite>("Image/IC_ITEM_DEFAULT");
    }

    void Start()
    {
        dataManager = DataManager.instance;
        
        SetRoster();
        SetSupportRoster();
        SetItemBox();   
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

    private void SetItemBox()
    {
        itemBoxMap.Clear();

        foreach(var i in itemBox)
        {
            if(!dataManager.itemData.TryGetValue(i.itemID, out var data))
            {
                Debug.LogWarning($"itemdata에 {i.itemID} 없음");
                continue;
            }

            i.cooltime = data.cooltime;
            
            if (!itemBoxMap.ContainsKey(i.itemID))
            {
                itemBoxMap.Add(i.itemID, i);
            }
        }
    }

    public Sprite GetItemIcon(string iconKey)
    {
        if(string.IsNullOrEmpty(iconKey)) return _iconCache["default"];
        if(_iconCache.TryGetValue(iconKey, out var sp)) return sp;

        sp = Resources.Load<Sprite>($"Image/{iconKey}");
        _iconCache[iconKey] = sp;
        return sp;
    }

    public void GetItem(string itemId, int count)
    {
        if(itemBoxMap.TryGetValue(itemId, out var item))
        {
            item.stack += count;
            return;
        }

        if(!dataManager.itemData.TryGetValue(itemId, out var data))
        {
            Debug.LogWarning($"itemdata에 {itemId} 없음");
            return;
        }

        ItemStack stack = new()
        {
            itemID = itemId,
            maxStack = data.maxStack,
            stack = count,
            cooltime = data.cooltime                
        };
        
        itemBox.Add(stack);
        itemBoxMap.Add(itemId, stack);
    }

    public void GetMoney(int count)
    {
        pcMoney += count;
    }
}