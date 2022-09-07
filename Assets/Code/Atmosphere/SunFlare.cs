using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SunFlare : MonoBehaviour
{
    public AtmosphereManagerNew atmo;
    
    public Vector2 minMaxIntensity = new Vector2(0.35f, 0.65f);
    public Vector4 todMinMax = new Vector4(0.25f, 0.4f, 0.63f, 0.78f);

    public Gradient colors;

    private SpriteRenderer myRenderer;

    void Start () {
        myRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        float tod = atmo.tod;

        if(tod <= todMinMax.x || tod > 0.78f) {
            myRenderer.color = Color.white * new Color(1f, 1f, 1f, minMaxIntensity.y);
        }
        else if(tod > todMinMax.x && tod <= todMinMax.y) {
            float intensity = minMaxIntensity.y - (minMaxIntensity.y - minMaxIntensity.x) * (tod - todMinMax.x) / (todMinMax.y - todMinMax.x);
            myRenderer.color = colors.Evaluate(tod) * new Color(1f, 1f, 1f, intensity);
        }
        else if(tod > todMinMax.y && tod <= todMinMax.z) {
            myRenderer.color = colors.Evaluate(tod) * new Color(1f, 1f, 1f, minMaxIntensity.x);
        }
        else {
            float intensity = (tod - todMinMax.z) / (todMinMax.w - todMinMax.z) * (minMaxIntensity.y - minMaxIntensity.x) + minMaxIntensity.x;
            myRenderer.color = colors.Evaluate(tod) * new Color(1f, 1f, 1f, intensity);
        }
    }
}
