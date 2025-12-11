using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleUIManager : MonoBehaviour
{
    private TurnGameManager gm;
    public Camera mainCam;
    public GameObject playerUIPannel;
    public SkillPannel skillUIPannel;
    public Button attackBtn;
    public Button skillBtn;
    public Button defendBtn;
    public Button itemBtn;

    private List<BattleUI> battleUIs = new();
    private bool isTargetSelectMode = false;
    private HashSet<BattleUnit> candidateSet = new();
    private Action<BattleUnit> onTargetSelected;
    private BattleUI currentHover;

    void Start()
    {
        gm = TurnGameManager.instance;

        gm.OnPlayerTurnStart += ShowPlayerUI;
        skillUIPannel.uiManager = this;

        HidePlayerUI();

        attackBtn.onClick.AddListener(() =>
        {
            gm.OnPlayerSelectCommand(TurnGameManager.BattleCommandType.Attack);
        });

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

        if(!isTargetSelectMode) return;

        Vector3 world = mainCam.ScreenToWorldPoint(Input.mousePosition);
        Vector2 pos = new(world.x,world.y);

        RaycastHit2D hit = Physics2D.Raycast(pos, Vector2.zero);
        BattleUI ui = null;

        if(hit.collider != null)
        {
            ui = hit.collider.GetComponent<BattleUI>();
            if(ui != null && !candidateSet.Contains(ui.runtimeUnit))
            {
                ui = null;
            }
        }

        if(currentHover != ui)
        {
            if(currentHover != null) currentHover.SetHover(false);
            currentHover = ui;
            if(currentHover != null) currentHover.SetHover(true);
        }

        if(Input.GetMouseButtonDown(0) && ui != null)
        {
            var unit = ui.runtimeUnit;
            if(unit != null && candidateSet.Contains(unit))
            {
                onTargetSelected?.Invoke(unit);
                ExitTargetSelectMode();
            }
        }
    }

    public void RegisterBattleUI(BattleUI ui)
    {
        if (!battleUIs.Contains(ui))
        {
            battleUIs.Add(ui);
        }
    }

    public void EnterTargetSelectMode(IEnumerable<BattleUnit> candidates, Action<BattleUnit> onSelected)
    {
        isTargetSelectMode = true;
        onTargetSelected = onSelected;
        candidateSet = new HashSet<BattleUnit>(candidates);

        foreach(var ui in battleUIs)
        {
            bool isCandidate = ui.runtimeUnit != null && candidateSet.Contains(ui.runtimeUnit);
            ui.SetCandidate(isCandidate);
            ui.SetHover(false);
        }
    }

    private void ExitTargetSelectMode()
    {
        isTargetSelectMode = false;
        onTargetSelected = null;
        candidateSet.Clear();

        foreach(var ui in battleUIs)
        {
            ui.SetCandidate(false);
            ui.SetHover(false);
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

    public void ForceExitSelectMode()
    {
        if(currentHover != null)
        {
            currentHover.SetHover(false);
            currentHover = null;
        }

        isTargetSelectMode = false;
        candidateSet.Clear();
        onTargetSelected = null;
    }

    private void OnDestroy()
    {
        gm.OnPlayerTurnStart -= ShowPlayerUI;
    }
}
