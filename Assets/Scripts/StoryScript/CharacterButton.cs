using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CharacterButton : MonoBehaviour, IBeginDragHandler,IDragHandler,IEndDragHandler
{
    private DataManager dataManager;
    public RosterUIManager uiManager;
    public PlayerCharacterStat stat;
    public Canvas canvas;
    public CanvasGroup group;
    [SerializeField] private Button btn;
    [SerializeField] private Image img;
    public GameObject skillPannel;
    [SerializeField] private DragGhost ghostPrefab;
    [SerializeField] private GameObject skillBtn;
    private List<GameObject> skillBtnList = new();
    public DragGhost ghost;
    public bool dropDragGhost;
    public PartySlot nowSlot;
    public string characterName;

    public void Init()
    {
        dataManager = DataManager.instance;
        
        if(dataManager.characterStats.TryGetValue(stat.characterID,out var character))
        {
            characterName = character.name;
        }

        btn.onClick.AddListener(() =>
        {
            SetSkillPannel();
        });
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if(nowSlot != null) return;
        dropDragGhost = false;
        group.alpha = 0.5f;

        ghost = Instantiate(ghostPrefab,canvas.transform);
        ghost.Init(this,characterName);

        var ghostGroup = ghost.GetComponent<CanvasGroup>();
        ghostGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if(ghost == null) return;
        ghost.transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if(ghost != null) Destroy(ghost.gameObject);

        if(!dropDragGhost) group.alpha = 1;
    }

    public void SelectCharacter(bool on)
    {
        Color color = Color.white;
        group.alpha = 1.0f;
        if (on)
        {
            color = Color.gray;
            group.alpha = 0.5f;
        }
        img.color = color;
    }

    private void MakeSelectBtn()
    {
        skillBtnList.Clear();
        foreach(var id in stat.learnedSkillID)
        {
            if(!dataManager.skillDatas.TryGetValue(id,out var skill))
            {
                Debug.LogWarning($"skilldata에 {id} 없음");
                continue;
            }
            GameObject obj = Instantiate(skillBtn, skillPannel.transform);
            skillBtnList.Add(obj);
            obj.GetComponentInChildren<TMP_Text>().text = skill.skillName;
            Button btn = obj.GetComponent<Button>();
            btn.onClick.AddListener(() =>
            {
                SelectSkill(btn,id);
            });
            LoadSelectSkill(btn,skill.skillID);
        }
        uiManager.SetNowSelectButton(this);
    }

    public void SetSkillPannel()
    {
        if (!skillPannel.activeSelf)
        {
            ShowSkillPannel();
        }
        else
        {
            ClearSkillButtons();

            if(uiManager.GetNowSelectButton() == this)
            {
                skillPannel.SetActive(false);
                uiManager.nowButton = null;
            }
            else
            {
                MakeSelectBtn();
            }
        }
    }

    public void ShowSkillPannel()
    {
        skillPannel.SetActive(true);
        ClearSkillButtons();
        MakeSelectBtn();
    }

    private void ClearSkillButtons()
    {
        if(uiManager.nowButton == null) return;
        foreach(var go in uiManager.nowButton.skillBtnList)
        {
            if(go != null) Destroy(go);
        }
        skillBtnList.Clear();
    }

    private void LoadSelectSkill(Button btn, string id)
    {
        if (stat.defaultEquippedSkillID.Contains(id))
        {
            btn.GetComponent<Image>().color = Color.gray;
        }
    }

    public void SelectSkill(Button btn, string id)
    {
        if (!stat.defaultEquippedSkillID.Contains(id))
        {
            if(stat.defaultEquippedSkillID.Count == 4)
            {
                Debug.Log("skill 편성 꽉참");
                return;
            }
            stat.defaultEquippedSkillID.Add(id);
            btn.GetComponent<Image>().color = Color.gray;
        }
        else
        {
            stat.defaultEquippedSkillID.Remove(id);
            btn.GetComponent<Image>().color = Color.white;
        }
    }
}
