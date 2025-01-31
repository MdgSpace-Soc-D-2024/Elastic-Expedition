using UnityEngine;
using UnityEngine.UI;

public class Deathhh : MonoBehaviour
{
    public float startBar=0f,fullbar=100f; // Maximum fuel the bike can have
    private float currentLength; // Current fuel level
    public float fuelConsumptionRate = 8f, fuelRefillRate = 2f; // Fuel consumed per second
    public Slider loadingBar; // Reference to the Slider UI element 
    public GameObject myPanel; // Drag your UI Panel here in the Inspector
    // public void TogglePanel()
    // {
    //     myPanel.SetActive(!myPanel.activeSelf);
    // }
    private void Start()
    {
        // Initialize fuel to the maximum value
        currentLength = fullbar;
        // Update the UI initially
        UpdateBarUI();
    }
    private void Update()
    {
        RaycastHit2D[]  hits = Physics2D.CircleCastAll(transform.position, 4f, transform.forward, 0.01f);
        if(hits.Length>0)
        {
             // Consume fuel as the bike moves
            EmptyBar();
            // Update UI each frame
            UpdateBarUI();
        }
        if(hits.Length==0){
            FillBar();
            UpdateBarUI();
        }
    }
    private void EmptyBar()
    {
        // Reduce fuel over time
        currentLength -= fuelConsumptionRate * Time.deltaTime;
        // Clamp fuel to a minimum of 0
        if (currentLength <=0f)
        {
            currentLength = 0f;
            GameOver(); // Call GameOver when out of fuel
        }
    }
     private void FillBar()
    {
        // Reduce fuel over time
        currentLength += fuelRefillRate * Time.deltaTime;
        // Clamp fuel to a minimum of 0
        if (currentLength >=99f)
        {
            currentLength = 100f;
        }
    }
    private void UpdateBarUI()
    {
        // Update the text to show current fuel level
        loadingBar.value = currentLength / 100f;
    }
    private void GameOver()
    {
        // Logic for when the player runs out of fuel
        Debug.Log("Out of fuel! Game Over!");
        // Freeze the game or reload the level, etc.
    }
}
