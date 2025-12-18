using TMPro;
using UnityEngine;

public class DragGhost : MonoBehaviour
{
    private TMP_Text text;
    public CharacterButton source;

    public void Init(CharacterButton s, string t)
    {
        source = s;
        text = GetComponentInChildren<TMP_Text>();
        if(text != null) text.text = t;
    }
}
