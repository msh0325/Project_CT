using TMPro;
using UnityEngine;

public class BattleUI : MonoBehaviour
{
    public TMP_Text nameText;
    public TMP_Text hpText;
    public BattleUnit runtimeUnit;

    public void Init(BattleUnit unit)
    {
        runtimeUnit = unit;
        Refresh();
    }

    public void Refresh()
    {
        if(runtimeUnit == null) return;

        if(nameText != null) nameText.text = runtimeUnit.name;

        if(hpText != null) hpText.text = $"HP : {runtimeUnit.currentHP}";
    }
}
