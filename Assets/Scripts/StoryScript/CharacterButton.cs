using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CharacterButton : MonoBehaviour, IBeginDragHandler,IDragHandler,IEndDragHandler
{
    private RectTransform rect;
    public PlayerCharacterStat stat;
    private Transform originalParent;
    private Vector2 anchoredPos;
    public PartySlot nowSlot;
    public bool isSelected = false;
    public string characterName;

    public void Init()
    {
        rect = GetComponent<RectTransform>();
        originalParent = rect.parent;
        anchoredPos = rect.anchoredPosition;
        if(DataManager.instance.characterStats.TryGetValue(stat.characterID,out var character))
        {
            characterName = character.name;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!isSelected)
        {
            anchoredPos = rect.anchoredPosition;
        }
        else
        {
            nowSlot.ClearSlot();
            nowSlot = null;
        }
        GetComponent<Image>().raycastTarget = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        GetComponent<Image>().raycastTarget = true;
        rect.SetParent(originalParent,false);
        rect.anchoredPosition = anchoredPos;
    }

    public void SelectCharacter(bool on)
    {
        isSelected = on;
        Color color = Color.white;
        if (on)
        {
            color = Color.gray;
        }
        GetComponent<Image>().color = color;
    }
}
