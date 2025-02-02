using UnityEngine;
using TMPro;
public class CoinUpdate : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public TMP_Text coinText;
    void Awake()
    {
        coinText.text= ""+PlayerPrefs.GetInt("TotalCoins", 0);
    }
    // void Start(){
    //     PlayerPrefs.SetInt("TotalCoins", 0);
    // }

    //Update is called once per frame
}
