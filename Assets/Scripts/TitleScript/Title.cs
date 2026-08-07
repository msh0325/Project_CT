using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Title : MonoBehaviour
{
    [SerializeField] private Button startBtn;
    [SerializeField] private Button exitBtn;
    void Start()
    {
        startBtn.onClick.AddListener(() =>
        {
            SceneManager.LoadScene("StoryScene");
        });

        exitBtn.onClick.AddListener(() =>
        {
            Application.Quit();
        });
    }
}
