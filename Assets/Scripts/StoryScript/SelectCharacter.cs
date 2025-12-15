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
    [SerializeField] private GameObject characterObj;
    void Start()
    {
        pcData = PlayerData.instance;
        dataManager = DataManager.instance;

        // roster에서 플레이어의 캐릭터들을 불러오고 최대 3명을 선택할 수 있게 구성하기.
        // 서포트 캐릭이나 편성 불가 캐릭 구현은 나중에
        // 나중에 제대로 ui 만들 때 드래그&드롭으로 파티 구성. 그때 rowtype 고를 수 있게 바꾸기
        foreach(var c in pcData.roster)
        {
            GameObject obj = Instantiate(characterObj,transform);

            string id = c.characterID;
            if(!dataManager.characterStats.TryGetValue(id,out var stat))
            {
                Debug.LogWarning($"캐릭터id {id}를 characterstats에서 찾을 수 없음");
                continue;
            }
            var statLocal = stat;

            obj.GetComponentInChildren<TMP_Text>().text = statLocal.name;

            /*obj.GetComponent<Button>().onClick.AddListener(() =>
            {
                if(pcData.selectedPartyMap.TryGetValue(cLocal.characterID,out var party))
                {
                    Debug.Log($"unselect {statLocal.name}");
                    pcData.selectedParty.Remove(party);
                    pcData.selectedPartyMap.Remove(party.characterID);
                    selectedCount--;
                    return;
                }
                if(selectedCount >= 3)
                {
                    Debug.Log($"최대 편성 수 3명 초과");
                    return;
                }

                Debug.Log($"select {statLocal.name}");
                PartyMemberSetting mem = new PartyMemberSetting
                {
                    characterID = cLocal.characterID,
                    row = RowSetting(),
                    battleEquippedSkillID = new List<string>(cLocal.defaultEquippedSkillID)
                };
                pcData.selectedParty.Add(mem);
                pcData.selectedPartyMap.Add(mem.characterID,mem);
                selectedCount++;
            });*/
        }
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            if(pcData.selectedParty.Count == 0)
            {
                Debug.Log("플레이어 캐릭터 선택 필요");
                return;
            }
            SceneManager.LoadScene("BattleScene");
        }
    }
}
