using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RewardSlot : MonoBehaviour
{
    public Image img;
    public TMP_Text text;
    
    public void SetSlot(Sprite sprite, string name)
    {
        img.sprite = sprite;
        text.text = name;
    }
}
