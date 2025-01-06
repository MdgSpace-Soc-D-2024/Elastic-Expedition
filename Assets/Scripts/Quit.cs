using UnityEngine;

public class ExitApp : MonoBehaviour
{
    public void ExitGame()
    {
        Application.Quit();
        #if UNITY_EDITOR
        Debug.Log("Application.Quit() called. App will close in a build.");
        #endif
    }
}