using TMPro;
using UnityEngine;

public class BattleUI : MonoBehaviour
{
    public TMP_Text nameText;
    public TMP_Text hpText;
    public TMP_Text mpText;
    public BattleUnit runtimeUnit;
    [SerializeField] private SpriteRenderer highlightBox;
    [SerializeField] private SpriteRenderer turnArrow;
    private Color targetColor = Color.yellow;
    private Color hoverColor = Color.red;

    private bool ishover = false;

    public void Init(BattleUnit unit)
    {
        runtimeUnit = unit;
        
        runtimeUnit.OnTurnStateChange -= UpdateTurnArrow;
        runtimeUnit.OnTurnStateChange += UpdateTurnArrow;
        
        Refresh();
    }

    public void Refresh()
    {
        if(runtimeUnit == null) return;

        if(nameText != null) nameText.text = runtimeUnit.name;

        if(hpText != null) hpText.text = $"HP : {runtimeUnit.currentHP}";

        if(mpText != null) mpText.text = $"MP : {runtimeUnit.currentMP}";
    }

    public void SetCandidate(bool on)
    {
        if(highlightBox == null) return;
        highlightBox.gameObject.SetActive(on);
        UpdateColor();
    }

    public void SetHover(bool on)
    {
        ishover = on;
        UpdateColor();
    }

    public void UpdateColor()
    {
        if(highlightBox == null) return;
        if(ishover) highlightBox.color = hoverColor;
        else highlightBox.color = targetColor;
    }
    
    public void UpdateTurnArrow(bool isCurrent, bool isNext)
    {
        if(isCurrent) ShowTurnArrow(true, true);
        else if(isNext) ShowTurnArrow(true, false);
        else ShowTurnArrow(false);
    }

    private void ShowTurnArrow(bool on, bool isNowTurn = false)
    {
        if(isNowTurn)
        {
            turnArrow.color = Color.green;
        }
        else
        {
            turnArrow.color = Color.red;
        }
        turnArrow.gameObject.SetActive(on);
    }

    private void OnDestroy()
    {
        TurnGameManager.instance.DeleteUIList(this);
        if(runtimeUnit != null)
        {
            runtimeUnit.OnTurnStateChange -= UpdateTurnArrow;
        }
    }
}
