using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicalFog : MonoBehaviour
{
    public AtmosphereManagerNew atmo;
    public Material fogMat;

    public Gradient todColors;

    private Color currentColor;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        currentColor = todColors.Evaluate(atmo.tod);
        currentColor.a *= (1f - atmo.fogIntensity);
        fogMat.SetColor("_MainColor", currentColor);
    }
}
