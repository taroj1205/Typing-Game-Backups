using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class StartMenu : MonoBehaviour
{
    public Button startButton;

    private void Start()
    {
        Button btn = startButton.GetComponent<Button>();
        btn.onClick.AddListener(StartGame);
        SceneManager.UnloadSceneAsync("Main");
    }

    public void StartGame()
    {
        Debug.Log("Starting game");
        SceneManager.UnloadSceneAsync("StartMenu");
        SceneManager.LoadScene("Main");
    }
}
