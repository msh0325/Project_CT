using System;
using System.Collections.Generic;
using TMPro;
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
    public Button supportBtn;

    private List<BattleUI> battleUIs = new();
    private bool isTargetSelectMode = false;
    private HashSet<BattleUnit> candidateSet = new();
    private Action<BattleUnit> onTargetSelected;
    private BattleUI currentHover;
    private TMP_Text supBtn_Text;
    private Image supBtn_Image;

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

        supportBtn.onClick.AddListener(() =>
        {
            gm.OnPlayerSelectCommand(TurnGameManager.BattleCommandType.Support);
        });
        
        supBtn_Text = supportBtn.GetComponentInChildren<TMP_Text>();
        supBtn_Image = supportBtn.GetComponent<Image>();
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
                if (!playerUIPannel.activeSelf)
                {
                    SwitchShowUI();
                }
                ExitTargetSelectMode();
            }
        }
        else if(Input.GetMouseButtonDown(0))
        {
            ExitTargetSelectMode();
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

    public void SwitchShowUI()
    {
        bool on = playerUIPannel.activeSelf;
        playerUIPannel.SetActive(!on);
        skillUIPannel.gameObject.SetActive(on);
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
        HidePlayerUI();
    }

    public void CheckCanUseSupport(SupportUnit support, BattleUnit unit)
    {
        if(support == null || unit == null)
        {
            SettingButtonUI(false,false,null);
            return;
        }
        bool canUseSubAction = unit.CanUseSubAction();
        bool isCooldown = support.CanUseSupport();
        bool canUse = canUseSubAction && isCooldown;

        if (!canUse)
        {
            string label = !isCooldown? support.cooldown.ToString() : "support";
            SettingButtonUI(false,true,label);
            return;
        }
        SettingButtonUI(true,false, "support");
    }

    public void SettingButtonUI(bool isActive, bool isGray, string label)
    {
        supportBtn.interactable = isActive;
        if(supBtn_Image != null) supBtn_Image.color = isGray ? Color.gray : Color.white;
        if(supBtn_Text != null) supBtn_Text.text = label;
    }

    private void OnDestroy()
    {
        gm.OnPlayerTurnStart -= ShowPlayerUI;
    }
}
