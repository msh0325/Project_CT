using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillPannel : MonoBehaviour
{
    private TurnGameManager gm;
    public BattleUIManager uiManager;
    private Button[] skillButtons;
    private SkillData[] skills;
    [SerializeField] private GameObject btnPrefab;
    private SkillButton btn;
    void Start()
    {
        gm = TurnGameManager.instance;
    }

    public void Init()
    {
        skillButtons = new Button[4];
        skills = new SkillData[4];
        for(int i = 0; i < skillButtons.Length; i++)
        {
            GameObject obj = Instantiate(btnPrefab,transform);
            skillButtons[i] = obj.GetComponent<Button>();
        }
    }

    public void SettingSkills(BattleUnit unit)
    {
        int btnCount = unit.partyChar.battleEquippedSkillID.Count;
        for(int i = 0; i < skillButtons.Length; i++)
        {
            if(i < unit.skills.Count)
            {
                string skillid = unit.partyChar.battleEquippedSkillID[i];
                SkillData skill = unit.skills[skillid];
                skills[i] = skill;
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

    public void CheckCanUseSkill(BattleUnit unit)
    {
        for(int i = 0; i < skillButtons.Length; i++)
        {
            btn = skillButtons[i].GetComponent<SkillButton>();
            if(!unit.CanUseSkill(skills[i].skillID))
            {
                //skillButtons[i].interactable = false;
                int cooltime = unit.GetSkillCoolTime(skills[i]);
                if(cooltime > 0)
                {
                    //skillButtons[i].GetComponentInChildren<TMP_Text>().text = cooltime.ToString();
                    btn.SetCooltime(true, cooltime);
                }
            }
            else
            {
                btn.SetCooltime(false,0);
                //skillButtons[i].interactable = true;
                //skillButtons[i].GetComponentInChildren<TMP_Text>().text = skills[i].skillName;
            }
        }
    }
}
