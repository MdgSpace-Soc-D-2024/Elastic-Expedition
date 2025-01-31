using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;

public class Load_Scene : MonoBehaviour
{
    public GameObject loadingscreen;
    public Slider slider;
    public Text progresstext;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void LoadLevel(string sceneName)
    {
        StartCoroutine(LoadAsynchoronously(sceneName));
    }
    IEnumerator LoadAsynchoronously(string sceneName)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        loadingscreen.SetActive(true);
        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            Debug.Log(operation.progress);
            slider.value = progress;
            progresstext.text = progress*100f +"%";
            yield return null; 
        }
    }
}
