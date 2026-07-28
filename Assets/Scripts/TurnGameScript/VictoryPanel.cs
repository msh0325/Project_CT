using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class VictoryPanel : MonoBehaviour
{
    [SerializeField] private Button nextBtn;
    [SerializeField] private GameObject rewardView;
    [SerializeField] private GameObject rewardPrefab;
    void Start()
    {
        nextBtn.onClick.AddListener(() =>
        {
            Debug.Log("다음으로");
            SceneManager.LoadScene("StoryScene");
        });
        
        gameObject.SetActive(false);
    }

    public void SetPanel(StageData data)
    {
        
    }
}
