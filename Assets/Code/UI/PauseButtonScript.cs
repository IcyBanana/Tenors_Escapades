using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class PauseButtonScript : MonoBehaviour
{
    // PauseButtonScript will be used to make the button appear when starting the game. Then after a short delay the button automatically hides, but is still touchable. It appears on click.
    public float hideDelay = 2f; // Delay in seconds before button hides.
    public float lerpSpeed = 2f; // Speed of image opacity lerp.
    public bool isShown = false;
    private Image myImage;
    
    void Start()
    {
        myImage = GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        ColorImage();
    }

    public void Appear () {
        isShown = true;
        if(hideDelay > 0)
            Invoke("Hide", hideDelay);
    }

    public void Hide () {
        isShown = false;
    }   

    void ColorImage () {
        float opacity = myImage.color.a;
        if(isShown) {
            opacity = Mathf.Lerp(opacity, 1f, lerpSpeed * Time.unscaledDeltaTime);
        }
        else {
            opacity = Mathf.Lerp(opacity, 0f, lerpSpeed * Time.unscaledDeltaTime);
        }
        myImage.color = new Color(myImage.color.r, myImage.color.g, myImage.color.b, opacity);
    }
}
