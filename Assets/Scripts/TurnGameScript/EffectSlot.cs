using UnityEngine;
using UnityEngine.UI;

public class EffectSlot : MonoBehaviour
{
    public Image effectImg;
    EffectData data;
    int effectValue;
    int effectDuration;
    string explain;
    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void Bind(EffectData e, int v, int d)
    {
        data = e;
        effectValue = v;
        effectDuration = d;
        explain = e.explain;
        effectImg.sprite = PlayerData.instance.GetIcon(e.iconId);
    }

    public void UpdateValue(int v, int d)
    {
        effectValue = v;
        effectDuration = d;
    }
}
