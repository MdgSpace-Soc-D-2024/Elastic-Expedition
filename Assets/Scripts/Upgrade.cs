using Unity.VisualScripting;
using UnityEngine;
using TMPro;

public class Upgrades : MonoBehaviour
{
    public TMP_Text Spring, Fuel, Engine, Rotation;
    public int count1 = 0,count2=0,count3,count4;
    public int upgradecoins_rot = 50;
    public int upgradecoins_engine =50;
    public int upgradecoins_spring = 50;
    public int upgradecoins_fuel = 50;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void check(string s)
    {
        if (s == "Rotation")
        {
            Rotation.text = "" + upgradecoins_rot;
            if (count1< 4)
            {
                
                if (PlayerPrefs.GetInt("TotalCoins", 0) > upgradecoins_rot)
                {
                    float val = PlayerPrefs.GetFloat("Rotation", 0.1f);
                    PlayerPrefs.SetFloat("Rotation", val + 0.3f);
                    PlayerPrefs.SetInt("TotalCoins", PlayerPrefs.GetInt("TotalCoins", 0) - upgradecoins_rot);
                    count1++;
                    upgradecoins_rot += 50 * (count1 + 1);
                    
                }
            }
        }
        if(s== "Engine")
        {
            Engine.text = "" + upgradecoins_engine;
            if (count2 < 4) 
            {
                
                if (PlayerPrefs.GetInt("TotalCoins", 0) > upgradecoins_engine)
                {
                    float val = PlayerPrefs.GetFloat("acc", 70);
                    float val2 = PlayerPrefs.GetFloat("brake", -90f);
                    int mv = PlayerPrefs.GetInt("MaxVelocity", 40);
                    PlayerPrefs.SetFloat("brake", val2 - 5f);
                    PlayerPrefs.SetFloat("acc", val + 5f);
                    PlayerPrefs.SetInt("MaxVelocity", mv + 3);
                    count2++;
                    upgradecoins_engine += 50 * (count2 + 1);
                    PlayerPrefs.SetInt("TotalCoins", PlayerPrefs.GetInt("TotalCoins", 0) - upgradecoins_engine);
                }
                else
                    Debug.Log("Not Enough Coins");
            }
        }
        if(s=="Spring")
        {
            Spring.text=""+upgradecoins_spring;
            if (count3 < 4)
            { 
                if(PlayerPrefs.GetInt("TotalCoins", 0)> upgradecoins_spring)
                {
                    float val = PlayerPrefs.GetFloat("SpConst", 50);
                    PlayerPrefs.SetFloat("SpConst", val + 10);
                    count3++;
                    upgradecoins_spring += 50 * (count3 + 1);
                    PlayerPrefs.SetInt("TotalCoins", PlayerPrefs.GetInt("TotalCoins", 0) - upgradecoins_spring);
                }
            }
        }
        if(s=="Fuel")
        {
            Fuel.text = "" + upgradecoins_fuel;
            if (count4 < 4)
            {
                if(PlayerPrefs.GetInt("TotalCoins", 0) > upgradecoins_fuel)
                {
                    float val = PlayerPrefs.GetFloat("Fuelrate", 5);
                    PlayerPrefs.SetFloat("Fuelrate",val - 0.5f);
                    count4++;
                    upgradecoins_fuel += 50 * (count4 + 1);
                    PlayerPrefs.SetInt("TotalCoins", PlayerPrefs.GetInt("TotalCoins", 0) - upgradecoins_fuel);

                }
            }
            
        }
    }
}
