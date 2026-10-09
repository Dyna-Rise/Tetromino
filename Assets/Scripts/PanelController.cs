using UnityEngine;
using UnityEngine.SceneManagement;

public class PanelController : MonoBehaviour
{
    public GameObject gameoverPanel;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void DisplayGameOverPanel()
    {
        gameoverPanel.SetActive(true);
    }
    public void Retry()
    {
        string sceneName  = SceneManager.GetActiveScene().name;
        //同じシーンを読み込んでリトライできる
        SceneManager.LoadScene(sceneName);
    }
}
