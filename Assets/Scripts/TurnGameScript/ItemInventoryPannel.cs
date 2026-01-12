using UnityEngine;

public class ItemInventoryPannel : MonoBehaviour
{
    private PlayerData pcData;
    [SerializeField] private GameObject itemSlotPrefab;
    private GameObject[] slots;
    private ItemSlot nowSelectSlot;
    public int slotNum = 16;

    void Awake()
    {
        slots = new GameObject[slotNum];

        for(int i = 0; i < slotNum; i++)
        {
            slots[i] = Instantiate(itemSlotPrefab,transform);
            slots[i].GetComponent<ItemSlot>().itemPannel = this;
            slots[i].GetComponent<ItemSlot>().SetEmpty();
        }
    }
    void Start()
    {
        SettingSlots();
    }

    void OnEnable()
    {
        pcData = PlayerData.instance;
        Refresh();
    }

    public void SettingSlots()
    {
        int index = 0;
        foreach(var itemStack in pcData.itemBoxMap)
        {
            if(index >= slotNum) break;

            ItemSlot slot = slots[index].GetComponent<ItemSlot>();
            slot.count_Text.text = itemStack.Value.ToString();
            DataManager.instance.itemData.TryGetValue(itemStack.Key, out var itemdata);
            
            slot.Bind(itemdata,itemStack.Key, itemStack.Value.stack);
            index++;
        }
    }

    public void Refresh()
    {
        for(int i = 0; i < slotNum; i++)
        {
            var slot = slots[i].GetComponent<ItemSlot>();
            slot.SetEmpty();
        }

        int index = 0;
        foreach(var kv in pcData.itemBoxMap)
        {
            if(index >= slotNum) break;

            DataManager.instance.itemData.TryGetValue(kv.Key, out var itemData);
            var slot = slots[index].GetComponent<ItemSlot>();
            int cooltime = kv.Value.nowCooltime;
            if(cooltime > 0)
            {
                slot.ShowCooltime(true, cooltime);
            }
            else
            {
                slot.ShowCooltime(false);
            }
            
            slot.Bind(itemData, kv.Key, kv.Value.stack);
            slot.SetButtonClick(kv.Value.CanUseItem());
            index++;
        }
    }

    public void SelectSlot(ItemSlot s)
    {
        if(nowSelectSlot != null)
        {
            nowSelectSlot.Selected(false);
        }

        nowSelectSlot = s;
        nowSelectSlot.Selected(true);
    }

    public void ClearSelectSlot()
    {
        if(nowSelectSlot != null)
        {
            nowSelectSlot.Selected(false);
        }

        nowSelectSlot = null;
    }
}
