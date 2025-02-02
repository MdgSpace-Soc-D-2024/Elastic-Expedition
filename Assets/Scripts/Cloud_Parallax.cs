using UnityEngine;

public class Cloud_Parallax : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public GameObject Cloud1,cloud2;
    public Front_wheel Script_F;
    public GameObject Fwheel;

    void Start()
    {
        Script_F= Fwheel.GetComponent<Front_wheel>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 velocity= Script_F.velocity;
        Cloud1.transform.position += new Vector3(velocity.x/1000f,0,0);
        cloud2.transform.position += new Vector3(velocity.x/1250f,0,0);
    }
}
