using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SunLightScript : MonoBehaviour
{
    public AtmosphereManagerNew atmosphereManager;
    public AnimationCurve todCurve;
    public float lightIntMax = 0.5f;
    public float lightIntMin = 0f;
    public Gradient lightColorGradient;
    public Vector2 sunriseRotations;
    public Vector2 middayRotations;
    public Vector2 sunsetRotations;
    private Light myLight;
    // Start is called before the first frame update
    void Start()
    {
        myLight = GetComponent<Light>();
    }

    // Update is called once per frame
    void Update()
    {
        float todAdjusted = Mathf.Clamp(((atmosphereManager.tod - 0.15f) / 0.7f), 0f, 1f);
        float intensity = todCurve.Evaluate(todAdjusted);
        myLight.intensity =  (lightIntMax - lightIntMin) * intensity + lightIntMin;
        myLight.color = lightColorGradient.Evaluate(todAdjusted);
        Vector3 eulerRotation;
        if(todAdjusted < 0.5f) 
            eulerRotation = Vector2.Lerp(sunriseRotations, middayRotations, todAdjusted * 2f);
        else
            eulerRotation = Vector2.Lerp(middayRotations, sunsetRotations, (todAdjusted - 0.5f) * 2f);
        transform.rotation = Quaternion.Euler(eulerRotation);
    }
}
