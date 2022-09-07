using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvSnowEmission : MonoBehaviour
{
    public Gradient color;
    public Material myMat;
    public Light mainLight;

    private float mult;


    void Start()
    {
        mult = 1f / mainLight.GetComponent<SunLightScript>().lightIntMax;
    }


    void Update()
    {
        float H;
        float S;
        float V;
        Color.RGBToHSV(mainLight.color, out H, out S, out V);

        Color currentColor = Color.HSVToRGB(H, S * 2f, Mathf.Clamp((V * mainLight.intensity * mult), 0f, 0.9f));
        
        myMat.SetColor("_EmissionColor", currentColor);
    }
}
