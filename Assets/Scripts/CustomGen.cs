using UnityEngine;
using UnityEngine.UI;
public class CustomLevel : MonoBehaviour
{
    public GameObject ConfigScreen;
    private bool isPaused = false;

    void Start()
    {
        ConfigScreen.SetActive(false);
    }

    public void ToggleActive()
    {
        isPaused = !isPaused;
        if(isPaused){
            ConfigScreen.SetActive(true);
        }
        else{
            ConfigScreen.SetActive(false);
        }
    }
}
