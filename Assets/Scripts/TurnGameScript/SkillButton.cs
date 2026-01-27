using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillButton : MonoBehaviour
{
    [SerializeField] private Button btn;
    [SerializeField] private GameObject cooltimeObj;
    [SerializeField] private TMP_Text cooltimeText;
    
    public void SetCooltime(bool on, int nowCooltime)
    {
        if (on)
        {
            cooltimeObj.SetActive(true);
            cooltimeText.text = nowCooltime.ToString();
            btn.interactable = false;            
        }
        else
        {
            cooltimeObj.SetActive(false);
            btn.interactable = true;
        }
    }
}
