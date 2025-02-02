using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingManager : MonoBehaviour
{
    public Slider loadingBar; // Reference to the Slider UI element
    public string sceneToLoad; // The scene to load (set this in the Inspector)
    public float loadTime = 5f; // Total time for loading (in seconds)

    void Start()
    {
        StartCoroutine(LoadSceneAsync());
    }

    IEnumerator LoadSceneAsync()
    {
        // Begin loading the scene in the background
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneToLoad);

        // Prevent the scene from activating immediately
        operation.allowSceneActivation = false;

        float elapsedTime = 0f;

        // While the scene is loading
        while (elapsedTime < loadTime)
        {
            // Calculate the progress based on the elapsed time
            
            float progress = Mathf.Clamp01(elapsedTime / loadTime);
            loadingBar.value = progress;

            // Increment the elapsed time
            elapsedTime += Time.deltaTime;

            yield return null; // Wait until the next frame
        }

        // Ensure the progress bar is fully filled after 5 seconds
        loadingBar.value = 1f;

        // Activate the scene once the loading is done
        operation.allowSceneActivation = true;

        // Wait until the scene is fully loaded
        while (!operation.isDone)
        {
            yield return null;
        }
    }
}