using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PartySlot : MonoBehaviour, IDropHandler
{   
    public CharacterButton character;
    public GameObject currentBtn;
    public GameObject slotBtn;
    public RowType row;

    private bool IsEmpty()
    {
        if(character == null) return true;
        else return false;
    }

    public void ClearSlot()
    {
        if(character != null)
        {
            character.SelectCharacter(false);
            character.nowSlot = null;
            character = null;
        }

        if(currentBtn != null)
        {
            Destroy(currentBtn);
            currentBtn = null;
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        var charBtn = eventData.pointerDrag?.GetComponent<CharacterButton>();
        if(charBtn == null) return;

        if (!IsEmpty())
        {
            character.SelectCharacter(false);

            if(currentBtn != null)
            {
                Destroy(currentBtn);
            }
        }
        
        SetUIFromRosterBtn(charBtn);
    }

    public void SetUIFromRosterBtn(CharacterButton charBtn)
    {
        if(charBtn == null) return;

        if(character != null) character.SelectCharacter(false);
        if(currentBtn != null) currentBtn = null;
        
        currentBtn = Instantiate(slotBtn,transform);
        currentBtn.transform.localPosition = Vector3.zero;

        var text = currentBtn.GetComponentInChildren<TMP_Text>();
        if(text != null) text.text = charBtn.characterName;

        var btn = currentBtn.GetComponent<Button>();
        if(btn != null) btn.onClick.AddListener(()=>ClearSlot());

        character = charBtn;
        character.nowSlot = this;
        character.SelectCharacter(true);
    }
}
