using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HouseLights : MonoBehaviour
{
    [ColorUsage(true, true)]
    public Color lightsColor;
    private AtmosphereManagerNew atmosphereManager;
    private Material myMat;
    private float lightsDelay; // Random delay per house to create more natural ambiance.


    void Start()
    {
        atmosphereManager = GameObject.Find("AtmosphereManager").GetComponent<AtmosphereManagerNew>();
        myMat = GetComponent<MeshRenderer>().material;
        lightsDelay = Random.Range(0, 0.1f);
    }

    void Update()
    {
        if(atmosphereManager) {
            if(!(atmosphereManager.tod > 0.22f - lightsDelay / 2f && atmosphereManager.tod < 0.8f + lightsDelay / 2f)) {
                myMat.SetColor("_EmissionColor", lightsColor);
            }
            else {
                myMat.SetColor("_EmissionColor", Color.black);
            }
        }
    }
}
