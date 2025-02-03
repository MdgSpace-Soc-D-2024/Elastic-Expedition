using UnityEngine;
using UnityEngine.UI;

public class ConfigLevel : MonoBehaviour
{
    public float heightMultiplier,Smoothness,Difficulty;
    public Slider HeightMul, Smooth, Diff; // Ensure these are assigned in the Inspector

    void Start()
    {
        heightMultiplier = PlayerPrefs.GetFloat("HeightMultiplier", 100); 
        HeightMul.value = (heightMultiplier - 100) / 300; // Ensure UI reflects stored value
        Smoothness = PlayerPrefs.GetFloat("Smoothness", 5f);
        Difficulty = PlayerPrefs.GetFloat("Difficulty", 2f);
    }

    void Update()
    {
        heightMultiplier = 100 + 300 * HeightMul.value;
        PlayerPrefs.SetFloat("HeightMultiplier", heightMultiplier); 
        Smoothness = 5f + 10f * Smooth.value;
        PlayerPrefs.SetFloat("Smoothness", Smoothness);
        Difficulty= 2f + 10f*Diff.value;
        PlayerPrefs.SetFloat("Difficulty", Difficulty);
        PlayerPrefs.Save(); // Save to disk
    }
}
