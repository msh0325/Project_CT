using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemSlot : MonoBehaviour
{
    public ItemInventoryPanel itemPannel;
    public GameObject cooltimeImg;
    public TMP_Text cooltime_Text;
    public TMP_Text count_Text;
    public Image itemImg;
    public ItemData itemData;
    public string itemID;
    public int itemCount;
    public Button btn;
    public Outline outline;

    void Awake()
    {
        btn.onClick.AddListener(() =>
        {
            if(itemData == null) return;
            
            itemPannel.SelectSlot(this);
            TurnGameManager.instance.OnPlayerSelectCommand(TurnGameManager.BattleCommandType.Item,null,itemData);
        });
    }

    public void SetEmpty()
    {
        itemData = null;
        itemID = null;
        itemCount = 0;

        count_Text.text = "";
        itemImg.sprite = null;
        count_Text.color = Color.black;
        outline.enabled = false;
    }

    public void Bind(ItemData data, string id, int c)
    {
        itemData = data;
        itemID = id;
        itemCount = c;

        count_Text.text = c.ToString();
        itemImg.sprite = PlayerData.instance.GetItemIcon(itemData.iconKey);

        
        if(itemCount <= 0)
        {
            count_Text.color = Color.red;
            outline.enabled = false;
            return;
        }

        count_Text.color = Color.black;
    }

    public void Selected(bool on)
    {
        outline.enabled = on;
    }

    public void SetButtonClick(bool on)
    {
        btn.interactable = on;
    }

    public void ShowCooltime(bool on, int cooltime = 0)
    {
        if (on)
        {
            cooltimeImg.SetActive(true);
            cooltime_Text.text = cooltime.ToString();
        }
        else
        {
            cooltimeImg.SetActive(false);
        }
    }
}
