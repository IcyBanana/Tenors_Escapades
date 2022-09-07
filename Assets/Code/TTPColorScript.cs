using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TTPColorScript : MonoBehaviour
{
    // Animates the color of the "Touch to start" UI component.

    // Atmo
    public AtmosphereManagerNew atmo;
    public Vector2 todDark = new Vector2(0.24f, 0.75f);

    // Colors
    public Color darkColor;
    public Color lightColor;

    // Sine Parameters
    public float sineLength = 2f;
    public AnimationCurve blinkCurve;
    public float sineIntensityLight = 0.8f;
    public float sineIntensityDark = 0.4f;

    // Lerp Timing
    public float lerpDuration = 0.1f;
    private float startTime;
    private bool isDark;

    private Text myText;
    private Image myImage;


    void Start()
    {
        myText = GetComponent<Text>();
        if(!myText) // If we don't have a text component, check for image.
            myImage = GetComponent<Image>();

        isDark = true;
    }

    void Update()
    {
        AdjustLightnessTOD();
        ChangeColor();     
    }

    void AdjustLightnessTOD () { // Adjusts isDark boolean based on time of day.
        float tod = atmo.tod;
        if(tod > todDark.x && tod < todDark.y) {
            if(!isDark) {
                isDark = true;
                startTime = Time.time;
            }
        }
        else {
            if(isDark) {
                isDark = false;
                startTime = Time.time;
            }
        }
    }

    void ChangeColor () { // Changes color based on isDark bool.
        float sineIntensity;
        if(isDark) {
            if(myText)
                myText.color = Color.Lerp(lightColor, darkColor, (Time.time - startTime) / lerpDuration);
            else if(myImage)
                myImage.color = Color.Lerp(lightColor, darkColor, (Time.time - startTime) / lerpDuration);
            sineIntensity = sineIntensityDark;
        }
        else {
            if(myText)
                myText.color = Color.Lerp(darkColor, lightColor, (Time.time - startTime) / lerpDuration);
            else if(myImage)
                myImage.color = Color.Lerp(darkColor, lightColor, (Time.time - startTime) / lerpDuration);
            sineIntensity = sineIntensityLight;
        }

        float newIntensity = 1f - blinkCurve.Evaluate((Time.time % sineLength) / sineLength) * sineIntensity;
        if(myText)
            myText.color = new Color(myText.color.r, myText.color.b, myText.color.g, newIntensity);
    }

    
}
