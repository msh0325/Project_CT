using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SelectCharacter : MonoBehaviour
{
    private PlayerData pcData;
    private DataManager dataManager;
    public RosterUIManager uiManager;
    private bool isInit = false;
    public Dictionary<string,CharacterButton> rosterBtnMap = new();
    [SerializeField] private GameObject characterObj;
    [SerializeField] private Button startBtn;
    [SerializeField] private GameObject rosterContent;
    [SerializeField] private GameObject selectSkill;
    public PartySlot[] slots;
    public TMP_Dropdown supports;
    private List<string> support_ids = new();
    void Start()
    {
        pcData = PlayerData.instance;
        dataManager = DataManager.instance;        
    }

    public void InitSelectCharacter()
    {
        if(isInit) return;
        
        MakeRosterCharacter();
        LoadPartyFromSlot();
        MakeSupportCharacter();
        
        startBtn.onClick.AddListener(() =>
        {
            SavePartyFromSlot();
            SaveSupportCharacter();
            if(CheckReadyToStart())
            {
                Debug.Log("start game");
                SceneManager.LoadScene("BattleScene");
            }
        });
        
        isInit = true;
    }

    // pcdata의 ownedCharacters를 바탕으로 로스터에 플레이어의 현재 보유 캐릭 생성
    private void MakeRosterCharacter()
    {
        foreach(var c in pcData.roster)
        {
            CharacterButton obj = Instantiate(characterObj,rosterContent.transform).GetComponent<CharacterButton>();

            string id = c.characterID;
            obj.canvas = GetComponentInParent<Canvas>();
            obj.skillPannel = selectSkill;
            obj.uiManager = uiManager;
            if(!dataManager.characterStats.TryGetValue(id,out var stat))
            {
                Debug.LogWarning($"캐릭터id {id}를 characterstats에서 찾을 수 없음");
                Destroy(obj.gameObject);
                continue;
            }
            var statLocal = stat;

            obj.GetComponentInChildren<TMP_Text>().text = statLocal.name;
            obj.stat = c;
            obj.Init();

            rosterBtnMap[id] = obj;
        }
    }

    // pcdata의 ownedSupports를 바탕으로 드롭다운에 플레이어의 현재 보유 서포트 캐릭 생성
    private void MakeSupportCharacter()
    {
        supports.ClearOptions();
        support_ids.Clear();

        var options = new List<TMP_Dropdown.OptionData>
        {
            new("선택 안함")
        };
        support_ids.Add(null);

        foreach(var id in pcData.ownedSupports)
        {
            if(!dataManager.supportData.TryGetValue(id,out var support))
            {
                Debug.LogWarning($"supportdata에 {id} 없음");
                continue;
            }
            support_ids.Add(id);
            options.Add(new TMP_Dropdown.OptionData(support.name));
        }

        supports.AddOptions(options);
        supports.value = 0;
        if(pcData.selectedSupport != null)
        {
            int index = support_ids.IndexOf(pcData.selectedSupport.supportID);
            supports.value = (index >=0)? index:0;
        }
        supports.RefreshShownValue();
    }

    // 슬롯에 편성된 캐릭터 pcdata에 저장
    private void SavePartyFromSlot()
    {
        pcData.selectedParty.Clear();
        pcData.selectedPartyMap.Clear();

        foreach(var s in slots)
        {
            if(s.character == null) continue;
            string id = s.character.stat.characterID;

            if(pcData.selectedPartyMap.ContainsKey(id)) continue;

            PartyMemberSetting mem = new PartyMemberSetting
            {
                characterID = id,
                row = s.row,
                battleEquippedSkillID = new List<string>(s.character.stat.defaultEquippedSkillID)
            };

            pcData.selectedParty.Add(mem);
            pcData.selectedPartyMap.Add(id,mem);
        }
    }

    // pcdata의 전에 편성한 캐릭터 미리 편성
    private void LoadPartyFromSlot()
    {
        foreach(var s in slots)
        {
            s.ClearSlot();
        }

        foreach(var mem in pcData.selectedParty)
        {
            var slot = slots.FirstOrDefault(s=>s.row == mem.row);
            if(slot == null) continue;

            if(!rosterBtnMap.TryGetValue(mem.characterID,out var charBtn))
            {
                Debug.LogWarning($"characterstat에 {mem.characterID}가 없음");
                continue;
            }

            slot.SetUIFromRosterBtn(charBtn);
        }
    }

    // 게임 시작을 위한 조건 체크
    private bool CheckReadyToStart()
    {
        int selectedCount = pcData.selectedParty.Count;
        if(string.IsNullOrEmpty(pcData.nowSelectStageID) || !dataManager.stageDatas.ContainsKey(pcData.nowSelectStageID))
        {
            Debug.LogWarning($"stagedata에 {pcData.nowSelectStageID}가 없음 또는 id가 비어있음.");
            return false;                
        }

        if(selectedCount <= 0)
        {
            Debug.Log("캐릭터 선택 필요");
            return false;
        }

        foreach(var party in pcData.selectedParty)
        {
            if(party.battleEquippedSkillID == null || party.battleEquippedSkillID.Count != 4)
            {
                Debug.Log($"{party.characterID}의 스킬 개수 부족 : {party.battleEquippedSkillID.Count} / 4");
                return false;
            }
        }

        return true;
    }

    // 선택된 서포트 캐릭터 pcdata에 저장
    private void SaveSupportCharacter()
    {
        int index = supports.value;
        if(index <= 0)
        {
            pcData.selectedSupport = null;
            return;
        }

        string id = support_ids[index];

        if(!dataManager.supportData.TryGetValue(id, out var support))
        {
            Debug.LogWarning($"supportdata에 {id} 없음");
            pcData.selectedSupport = null;
            return;
        }
        pcData.selectedSupport = support;
    }
}
