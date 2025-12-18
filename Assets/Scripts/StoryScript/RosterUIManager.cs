using UnityEngine;

public class RosterUIManager : MonoBehaviour
{
    public CharacterButton nowButton;
    [SerializeField] private SelectCharacter select;
    [SerializeField] private GameObject roster;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            SetRosterUI();
        }
    }

    public void SetRosterUI()
    {
        if (roster.activeSelf)
        {
            roster.SetActive(false);
        }
        else
        {
            roster.SetActive(true);
            select.InitSelectCharacter();
        }
    }
    
    public void SetNowSelectButton(CharacterButton btn)
    {
        nowButton = btn;
    }

    public CharacterButton GetNowSelectButton()
    {
        return nowButton;
    }
}