using UnityEngine;
using UnityEngine.UI;

public class FuelManager : MonoBehaviour
{
    public float maxFuel = 100f; // Maximum fuel the bike can have
    private float currentFuel;   // Current fuel level
    public float fuelConsumptionRate = 1f; // Fuel consumed per second
    public Slider loadingBar; // Reference to the Slider UI element   
    public GameObject GameOverPanel;
    
    private void Start()
    {
        // Initialize fuel to the maximum value
        currentFuel = maxFuel;

        // Update the UI initially
        UpdateFuelUI();
    }

    private void Update()
    {
        // Consume fuel as the bike moves
        ConsumeFuel();

        DetectFuelPickups();
        // Update UI each frame
        UpdateFuelUI();
    }

    private void ConsumeFuel()
    {
        // Reduce fuel over time4
        if(Input.GetAxis("Horizontal")==0f&&Input.GetAxis("Vertical")!=0f)
            fuelConsumptionRate=6f;
        else if(Input.GetAxis("Horizontal")!=0f||Input.GetAxis("Vertical")!=0f)
            fuelConsumptionRate=4f;
        else
            fuelConsumptionRate=1f;
        currentFuel -= fuelConsumptionRate * Time.deltaTime;
        // Clamp fuel to a minimum of 0
        if (currentFuel < 0f)
        {
            currentFuel = -5f;
            GameOver(); // Call GameOver when out of fuel
        }
    }

    private void UpdateFuelUI()
    {
        // Update the text to show current fuel level
        loadingBar.value = currentFuel / maxFuel;
    }

    private void GameOver()
    {
        GameOverPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    private void DetectFuelPickups()
    {
        // Perform a CircleCastAll to detect nearby pickups
        RaycastHit2D[]  hits = Physics2D.CircleCastAll(transform.position, 10f, transform.forward, 0.01f,LayerMask.GetMask("Fuel"));

        // Loop through all detected objects
        foreach (var hit in hits)
        {
            if (hit.collider != null && hit.collider.CompareTag("FuelPickup"))
            {
                Refuel();
                Destroy(hit.collider.gameObject); // Destroy the fuel pickup
            }
        }
    }

    private void Refuel()
    {
        // Add fuel, clamped to maxFuel
        currentFuel = 100f;
    }
}
