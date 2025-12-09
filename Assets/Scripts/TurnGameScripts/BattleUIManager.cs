using UnityEngine;
using UnityEngine.UI;

public class BattleUIManager : MonoBehaviour
{
    public TurnGameManager gm;
    public GameObject playerUIPannel;
    public Button attackBtn;
    public Button skillBtn;
    public Button defendBtn;
    public Button itemBtn;
    void Start()
    {
        attackBtn.onClick.AddListener(() =>
        {
            gm.OnPlayerSelectCommand(TurnGameManager.BattleCommandType.Attack);
        });

        skillBtn.onClick.AddListener(() =>
        {
            gm.OnPlayerSelectCommand(TurnGameManager.BattleCommandType.Skill);
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

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ShowPlayerUI()
    {
        playerUIPannel.SetActive(true);
    }

    public void HidePlayerUI()
    {
        playerUIPannel.SetActive(false);
    }
}
