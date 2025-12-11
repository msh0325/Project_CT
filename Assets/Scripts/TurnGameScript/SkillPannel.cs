using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillPannel : MonoBehaviour
{
    private TurnGameManager gm;
    public BattleUIManager uiManager;
    private Button[] skillButtons;
    [SerializeField] private GameObject btnPrefab;
    void Start()
    {
        gm = TurnGameManager.instance;
    }

    public void Init()
    {
        skillButtons = new Button[4];
        for(int i = 0; i < skillButtons.Length; i++)
        {
            GameObject obj = Instantiate(btnPrefab,transform);
            skillButtons[i] = obj.GetComponent<Button>();
        }
    }

    public void SettingSkills(BattleUnit unit)
    {
        int btnCount = unit.skills.Count;
        for(int i = 0; i < skillButtons.Length; i++)
        {
            if(i < unit.skills.Count)
            {
                SkillData skill = unit.skills[i];
                skillButtons[i].GetComponentInChildren<TMP_Text>().text = skill.skillName;
                skillButtons[i].onClick.RemoveAllListeners();
                skillButtons[i].onClick.AddListener(() =>
                {
                    gm.OnPlayerSelectCommand(TurnGameManager.BattleCommandType.Skill,skill);
                });
            }
            else
            {
                skillButtons[i].gameObject.SetActive(false);
            }
        }
    }
}
