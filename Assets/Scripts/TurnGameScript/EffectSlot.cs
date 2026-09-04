using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EffectSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image effectImg;
    public GameObject explainPanel;
    public TMP_Text valueText;
    public TMP_Text explainText;
    void Start()
    {
        explainPanel.SetActive(false);
        gameObject.SetActive(false);
    }

    public void Bind(EffectData e, int v, int d)
    {
        effectImg.sprite = PlayerData.instance.GetIcon(e.iconId);

        string txt = "";

        if(EffectPipeline.IsPercentValue(e.type))
        {
            txt = $"{v}% / {d}";
        }
        else if(v == 0)
        {
            txt = d.ToString();
        }
        else
        {
            txt = $"{v} / {d}";
        }
        valueText.text = txt;
        explainText.text = e.explain;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        explainPanel.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        explainPanel.SetActive(false);
    }
}
