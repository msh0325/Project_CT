using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CharacterButton : MonoBehaviour, IBeginDragHandler,IDragHandler,IEndDragHandler
{
    public Canvas canvas;
    private RectTransform rect;
    //private Vector3 originalPos;
    private int originalIndex;
    private Transform originalParent;
    public PartySlot nowSlot;
    public bool onPosition = false;

    void Start()
    {
        rect = GetComponent<RectTransform>();
        //originalPos = rect.position;
        originalIndex = rect.GetSiblingIndex();
        originalParent = rect.parent;
    }

    void Update()
    {
        
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        GetComponent<Image>().raycastTarget = false;

        /*if(nowSlot != null)
        {
            nowSlot.ClearSlot();
            nowSlot = null;
        }*/

        if(canvas !=null) rect.SetParent(canvas.transform, true);
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        GetComponent<Image>().raycastTarget = true;
        
        if(nowSlot != null)
        {
            RectTransform slotRect = nowSlot.GetComponent<RectTransform>();

            bool stillOnSlot = RectTransformUtility.RectangleContainsScreenPoint(
                slotRect,
                eventData.position,
                eventData.pressEventCamera
            );

            if (stillOnSlot)
            {
                rect.SetParent(nowSlot.transform);
                rect.position = nowSlot.transform.position;
                return;
            }
            
            nowSlot.ClearSlot();
            nowSlot = null;

            rect.SetParent(originalParent);
            rect.SetSiblingIndex(originalIndex);
            //rect.position = originalPos;
            return;
        }

        if(nowSlot == null)
        {
            rect.SetParent(originalParent);
            //rect.position = originalPos;
            rect.SetSiblingIndex(originalIndex);
        }
    }
}
