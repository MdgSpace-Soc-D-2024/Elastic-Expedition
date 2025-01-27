using UnityEngine;
using UnityEngine.UI;

public class ButtonSpriteChanger : MonoBehaviour
{
    public Button targetButton1,targetButton2,SpringCompress,SpringExpand ;     // The button to modify
    public Sprite normalSprite1,normalSprite2,normal_spring_compress,normal_spring_expand;    // The default sprite
    public Sprite pressedSprite1,pressedSprite2,pressed_spring_compress,pressed_spring_expand;   // The sprite to show when "D" is pressed

    private Image buttonImage1,buttonImage2,buttonImage_Spring1,buttonImage_Spring2;     // Reference to the button's Image component

    void Start()
    {
        // Get the Image component from the button
        buttonImage1 = targetButton1.GetComponent<Image>();
        buttonImage2 = targetButton2.GetComponent<Image>();
        buttonImage_Spring1 = SpringCompress.GetComponent<Image>();
        buttonImage_Spring2 = SpringExpand.GetComponent<Image>();

        // Set the initial sprite to the normal sprite
        buttonImage1.sprite = normalSprite1;
        buttonImage2.sprite = normalSprite2;
        buttonImage_Spring1.sprite = normal_spring_compress;
        buttonImage_Spring2.sprite = normal_spring_expand;
    }

    void FixedUpdate()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float VerticalInput = Input.GetAxis("Vertical");
        // Check if the "D" key is pressed
        if (horizontalInput==1)
        {
            buttonImage1.sprite = pressedSprite1;
        }
        else if(horizontalInput == -1){
            buttonImage2.sprite = pressedSprite2;
        }
        else
        {
            // Revert to the normal sprite when "D" is released
            buttonImage1.sprite = normalSprite1;
            buttonImage2.sprite = normalSprite2;
        }
        if (VerticalInput == 1)
        {
            buttonImage_Spring1.sprite = pressed_spring_expand;
        }
        else if (VerticalInput == -1)
        {
            buttonImage_Spring2 .sprite = pressed_spring_compress;
        }
        else
        {
            // Revert to the normal sprite when "D" is released
            buttonImage_Spring1.sprite = normal_spring_expand;
            buttonImage_Spring2.sprite = normal_spring_compress;
        }
    }
}