using UnityEngine;

public class SelectCharacter : MonoBehaviour
{
    private PlayerData pcData;
    void Start()
    {
        pcData = PlayerData.instance;
    }
    void Update()
    {
        // roster에서 플레이어의 캐릭터들을 불러오고 최대 3명을 선택할 수 있게 구성하기.
        // 서포트 캐릭이나 편성 불가 캐릭 구현은 나중에
    }
}
