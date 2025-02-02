using UnityEngine;

public class Upgrades : MonoBehaviour
{
    public int count = 0;
    public void Start(){
        PlayerPrefs.SetFloat("Rotation", 0.1f);
        PlayerPrefs.SetFloat("acc", 70);
        PlayerPrefs.SetFloat("brake", -90f);
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void check(string s)
    {
        if (s == "Rotation")
        {
            if (count < 4)
            {
                float val = PlayerPrefs.GetFloat("Rotation", 0.1f);
                PlayerPrefs.SetFloat("Rotation", val + 0.3f);
                count++;
            }
        }
        if(s== "Engine")
        {
            if (count < 4)
            {
                float val = PlayerPrefs.GetFloat("acc", 70);
                float val2 = PlayerPrefs.GetFloat("brake", -90f);
                PlayerPrefs.SetFloat("brake", val2 - 20f);
                PlayerPrefs.SetFloat("acc",val + 20f);
            }
        }
    }
}
