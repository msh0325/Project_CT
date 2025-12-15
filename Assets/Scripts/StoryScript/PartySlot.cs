using UnityEngine;
using UnityEngine.EventSystems;

public class PartySlot : MonoBehaviour, IDropHandler
{   
    public CharacterButton character;
    public RowType row;
    void Start()
    {
        
    }

    void Update()
    {
        
    }

    private bool IsEmpty()
    {
        if(character == null) return true;
        else return false;
    }

    public void ClearSlot()
    {
        character = null;
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (IsEmpty())
        {
            var btn = eventData.pointerDrag?.GetComponent<CharacterButton>();
            if(btn == null) return;
            
            character = btn;
            btn.nowSlot = this;

            btn.transform.SetParent(transform);
            btn.GetComponent<RectTransform>().position = transform.position;
        }
    }
}
