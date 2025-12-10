using UnityEngine;
using UnityEngine.UI;

public class BattleUIManager : MonoBehaviour
{
    private TurnGameManager gm;
    public GameObject playerUIPannel;
    public SkillPannel skillUIPannel;
    public Button attackBtn;
    public Button skillBtn;
    public Button defendBtn;
    public Button itemBtn;

    void Start()
    {
        gm = TurnGameManager.instance;

        gm.OnPlayerTurnStart += ShowPlayerUI;
        skillUIPannel.uiManager = this;

        playerUIPannel.SetActive(false);
        skillUIPannel.gameObject.SetActive(false);
        attackBtn.onClick.AddListener(() =>
        {
            gm.OnPlayerSelectCommand(TurnGameManager.BattleCommandType.Attack);
        });

        // 나중에 스킬 여러개 중 하나 선택하면 그거 작동되도록 바꾸기
        skillBtn.onClick.AddListener(() =>
        {
            playerUIPannel.SetActive(false);
            skillUIPannel.gameObject.SetActive(true);
        });

        defendBtn.onClick.AddListener(() =>
        {
            gm.OnPlayerSelectCommand(TurnGameManager.BattleCommandType.Defend);
        });

        itemBtn.onClick.AddListener(() =>
        {
            gm.OnPlayerSelectCommand(TurnGameManager.BattleCommandType.Item);
        });
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape) && skillUIPannel.gameObject.activeSelf)
        {
            playerUIPannel.SetActive(true);
            skillUIPannel.gameObject.SetActive(false);
        }
    }

    public void ShowPlayerUI(BattleUnit unit)
    {
        playerUIPannel.SetActive(true);
        
        skillUIPannel.SettingSkills(unit);
    }

    public void HidePlayerUI()
    {
        playerUIPannel.SetActive(false);
        skillUIPannel.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        gm.OnPlayerTurnStart -= ShowPlayerUI;
    }
}
