using UnityEngine;
using UnityEngine.UI;

public class ButtonSpriteChanger : MonoBehaviour
{
    public Button targetButton1,targetButton2;     // The button to modify
    public Sprite normalSprite1,normalSprite2;    // The default sprite
    public Sprite pressedSprite1,pressedSprite2;   // The sprite to show when "D" is pressed

    private Image buttonImage1,buttonImage2;     // Reference to the button's Image component

    void Start()
    {
        // Get the Image component from the button
        buttonImage1 = targetButton1.GetComponent<Image>();
        buttonImage2 = targetButton2.GetComponent<Image>();

        // Set the initial sprite to the normal sprite
        buttonImage1.sprite = normalSprite1;
        buttonImage2.sprite = normalSprite2;
    }

    void Update()
    {
        // Check if the "D" key is pressed
        if (Input.GetKey(KeyCode.D))
        {
            buttonImage1.sprite = pressedSprite1;
        }
        else if(Input.GetKey(KeyCode.A)){
            buttonImage2.sprite = pressedSprite2;
        }
        else
        {
            // Revert to the normal sprite when "D" is released
            buttonImage1.sprite = normalSprite1;
            buttonImage2.sprite = normalSprite2;
        }
    }
}