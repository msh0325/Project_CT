using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DefeatPanel : MonoBehaviour
{
    [SerializeField] private Button retryBtn;
    [SerializeField] private Button returnBtn;
    private string nowSceneName;
    void Start()
    {
        nowSceneName = SceneManager.GetActiveScene().name;

        retryBtn.onClick.AddListener(() =>
        {
            Debug.Log("다시 시도");
            SceneManager.LoadScene(nowSceneName);            
        });

        returnBtn.onClick.AddListener(() =>
        {
            Debug.Log("돌아가기")            ;
            SceneManager.LoadScene("StoryScene");
        });
        gameObject.SetActive(false);
    }
}
