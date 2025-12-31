using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemSlot : MonoBehaviour
{
    public TMP_Text count_Text;
    public Image itemImg;
    public ItemData itemData;
    public string itemID;
    public int itemCount;
    public Button btn;
    public Outline outline;

    void Start()
    {
        btn.onClick.AddListener(() =>
        {
            if(itemData == null) return;
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
        //btn.interactable = false;
        count_Text.color = Color.black;
    }

    public void Bind(ItemData data, string id, int c)
    {
        itemData = data;
        itemID = id;
        itemCount = c;

        count_Text.text = c.ToString();
        itemImg.sprite = PlayerData.instance.GetItemIcon(itemData.iconKey);
        btn.interactable = true;
        count_Text.color = Color.black;
    }
}
