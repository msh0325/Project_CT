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
    public Dictionary<string,CharacterButton> rosterBtnMap = new();
    [SerializeField] private GameObject characterObj;
    [SerializeField] private Button startBtn;
    public PartySlot[] slots;
    public TMP_Dropdown supports;
    void Start()
    {
        pcData = PlayerData.instance;
        dataManager = DataManager.instance;

        // roster에서 플레이어의 캐릭터들을 불러오고 최대 3명을 선택할 수 있게 구성하기.
        // 서포트 캐릭이나 편성 불가 캐릭 구현은 나중에
        // 나중에 제대로 ui 만들 때 드래그&드롭으로 파티 구성. 그때 rowtype 고를 수 있게 바꾸기
        foreach(var c in pcData.roster)
        {
            CharacterButton obj = Instantiate(characterObj,transform).GetComponent<CharacterButton>();

            string id = c.characterID;
            if(!dataManager.characterStats.TryGetValue(id,out var stat))
            {
                Debug.LogWarning($"캐릭터id {id}를 characterstats에서 찾을 수 없음");
                continue;
            }
            var statLocal = stat;

            obj.GetComponentInChildren<TMP_Text>().text = statLocal.name;
            obj.stat = c;
            obj.Init();

            rosterBtnMap[id] = obj;
        }

        LoadPartyFromSlot();
        
        startBtn.onClick.AddListener(() =>
        {
            SavePartyFromSlot();
            SaveSupportCharacter();

            int selectedCount = pcData.selectedParty.Count;
            if(selectedCount > 0)
            {
                Debug.Log("start game");
                SceneManager.LoadScene("BattleScene");
            }
            else if(selectedCount <= 0)
            {
                Debug.Log("캐릭터 선택 필요");
            }
        });

        supports.AddOptions(pcData.ownedSupports);
    }

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

    private void SaveSupportCharacter()
    {
        int index = supports.value;
        string id = supports.options[index].text;

        if(!dataManager.supportData.TryGetValue(id,out var sup))
        {
            Debug.LogWarning($"supportdata에서 {id} 없음");
            pcData.selectedSupport = null;
            return;
        }
        pcData.selectedSupport = sup;
    }
}
