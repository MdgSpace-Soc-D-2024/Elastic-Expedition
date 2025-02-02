using UnityEngine;

public class SnowFall : MonoBehaviour
{   
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public GameObject SnowFalll;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        SnowFalll.transform.localPosition=transform.localPosition+new Vector3(0,50,0);
    }
}
