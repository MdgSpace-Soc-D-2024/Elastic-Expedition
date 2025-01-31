using UnityEngine;
using UnityEngine.UI;

public class PauseManager : MonoBehaviour
{
    public Button pauseButton;
    public GameObject PauseScreen;
    private bool isPaused = false;

    void Start()
    {
        PauseScreen.SetActive(false);
    }

    public void TogglePause()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;
        AudioListener.pause = isPaused; // Pause all audio when game is paused
        Debug.Log("Pause Toggled: " + isPaused);
        if(isPaused){
            PauseScreen.SetActive(true);
        }
        else{
            PauseScreen.SetActive(false);
        }
    }
}
