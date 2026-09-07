using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleUI : MonoBehaviour
{
    public SpriteRenderer unitSprite;
    public SpriteRenderer outlineSprite; // outline 위한 스프라이트 따로
    public Canvas canvas;
    public Image hpSlider;
    public TMP_Text hpText;
    public Image mpSliter;
    public TMP_Text mpText;
    public BattleUnit runtimeUnit;
    [SerializeField] private SpriteRenderer highlightBox;
    [SerializeField] private SpriteRenderer turnArrow;
    [SerializeField] private EffectSlot[] effectSlots;
    private Color targetColor = Color.white;
    private Color hoverColor = Color.red;

    private bool ishover = false;
    private bool isOutlineOn = false;
    private static readonly int  OutlineEnabledID = Shader.PropertyToID("_OutlineEnable"); //_OutlineEnabled
    private static readonly int OutlineColorID = Shader.PropertyToID("_OutlineColor"); //_SolidOutline
    private MaterialPropertyBlock propBlock;

    void Awake()
    {
        propBlock = new MaterialPropertyBlock();
    }

    public void Init(BattleUnit unit)
    {
        runtimeUnit = unit;
        
        runtimeUnit.OnTurnStateChange -= UpdateTurnArrow;
        runtimeUnit.OnTurnStateChange += UpdateTurnArrow;

        if(unit.team == TeamType.Enemy)
        {
            //unitSprite.flipX = true;
            unitSprite.transform.localScale = new Vector3(-1.5f, 1.5f, 1.5f);
        }
        
        Refresh();
        canvas.worldCamera = Camera.main;
    }

    public void Refresh()
    {
        if(runtimeUnit == null) return;

        if(hpSlider != null)
        {
            hpSlider.fillAmount = (float)runtimeUnit.currentHP / runtimeUnit.maxHP;
        }

        if(hpText != null) hpText.text = $"{runtimeUnit.currentHP} / {runtimeUnit.maxHP}";

        if(mpSliter != null)
        {
            mpSliter.fillAmount = (float)runtimeUnit.currentMP / runtimeUnit.maxMP;
        }

        if(mpText != null) mpText.text = $"{runtimeUnit.currentMP} / {runtimeUnit.maxMP}";

        if(effectSlots != null)
        {
            var linkedIDs = DataManager.instance.linkedTargetIDs;
            int slotIdx = 0;
            
            foreach(var ae in runtimeUnit.activeEffects)
            {
                if(linkedIDs.Contains(ae.data.effectID)) continue;
                if(slotIdx >= effectSlots.Length) break;

                bool isToken = EffectPipeline.IsTokenBased(ae.data.type);

                effectSlots[slotIdx].gameObject.SetActive(true);
                effectSlots[slotIdx].Bind(ae.data, isToken?-1 : ae.value, ae.token>0?ae.token:ae.duration);
                slotIdx++;
            }

            for(int i=slotIdx; i<effectSlots.Length;i++)
            {
                effectSlots[i].gameObject.SetActive(false);
            }
        }
    }

    public void SetCandidate(bool on)
    {
        /*if(highlightBox == null) return;
        highlightBox.gameObject.SetActive(on);
        UpdateColor();*/

        if (isOutlineOn == on) return;

        isOutlineOn = on;

        outlineSprite.GetPropertyBlock(propBlock);
        propBlock.SetFloat(OutlineEnabledID, on ? 1.0f : 0.0f);
        outlineSprite.SetPropertyBlock(propBlock);
        UpdateColor();
    }

    public void SetHover(bool on)
    {
        ishover = on;
        UpdateColor();
    }

    public void UpdateColor()
    {
        outlineSprite.GetPropertyBlock(propBlock);
        if (ishover) propBlock.SetColor(OutlineColorID, hoverColor);
        else propBlock.SetColor(OutlineColorID, targetColor);
        outlineSprite.SetPropertyBlock(propBlock);
        //if(ishover) highlightBox.color = hoverColor;
        //else highlightBox.color = targetColor;
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
