using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentLightScript : MonoBehaviour
{
    public AtmosphereManagerNew atmosphereManager;
    public AnimationCurve todCurve;
    public float lightIntMax = 0.5f;
    public Gradient lightColorGradient;
    private Light myLight;
    // Start is called before the first frame update
    void Start()
    {
        myLight = GetComponent<Light>();
    }

    // Update is called once per frame
    void Update()
    {
        float todAdjusted = atmosphereManager.tod;
        float intensity = todCurve.Evaluate(todAdjusted);
        myLight.intensity = lightIntMax * intensity;
        myLight.color = lightColorGradient.Evaluate(todAdjusted);
    }
}
