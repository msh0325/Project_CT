using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class VictoryPanel : MonoBehaviour
{
    [SerializeField] private Button nextBtn;
    [SerializeField] private GameObject rewardView;
    [SerializeField] private GameObject rewardPrefab;
    [SerializeField] private TMP_Text moneyText;
    void Start()
    {
        nextBtn.onClick.AddListener(() =>
        {
            Debug.Log("다음으로");
            SceneManager.LoadScene("StoryScene");
        });
    }

    public void SetPanel(StageData data)
    {
        for(int i=0;i<data.rewardItemID.Length;i++)
        {
            string rewardId = data.rewardItemID[i];
            int iconCount = data.rewardItemcount[i];

            RewardSlot slot = Instantiate(rewardPrefab, rewardView.transform).GetComponent<RewardSlot>();

            string itemName = DataManager.instance.itemData[rewardId].name;
            string iconID = DataManager.instance.itemData[rewardId].iconKey;
            
            Sprite s = PlayerData.instance.GetItemIcon(iconID);
            string t = itemName + " X " + iconCount;

            slot.SetSlot(s,t);
            PlayerData.instance.GetItem(rewardId, iconCount);
        }

        moneyText.text = $"획득한 돈 : {data.rewardMoney}";
    }
}
