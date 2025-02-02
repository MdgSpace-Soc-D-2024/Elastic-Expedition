using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;
public class CoinManager : MonoBehaviour
{
    public TMP_Text inGameCoinsText;  // Assign in Inspector
    private int currentCoins = 0;
    private int totalCoins = 0;
    public GameObject Car;
    public TMP_Text GameOverCoinsText;

    void Start()
    {
        totalCoins = PlayerPrefs.GetInt("TotalCoins", 0);
        
        UpdateUI();
    }
        private void DetectCoinPickups()
    {
        // Perform a CircleCastAll to detect nearby pickups
        RaycastHit2D[]  hits = Physics2D.CircleCastAll(Car.transform.position, 5f, transform.forward, 0.01f,LayerMask.GetMask("Coin"));

        // Loop through all detected objects
        foreach (var hit in hits)
        {
            if (hit.collider != null)
            {
                Destroy(hit.collider.gameObject); // Destroy the fuel pickup
                CollectCoin(1);
                totalCoins +=1;
                PlayerPrefs.SetInt("TotalCoins", totalCoins);
            }
        }
    }
   
    void UpdateUI()
    {
        inGameCoinsText.text =""+currentCoins;
        GameOverCoinsText.text ="Coins:"+currentCoins;
    }

    // Call this when player collects a coin
    public void CollectCoin(int amount)
    {
        currentCoins += amount;
        UpdateUI();
    }

    // Call this when level ends (Save total coins)
     void FixedUpdate(){
        DetectCoinPickups();
        UpdateUI();
    }


}
