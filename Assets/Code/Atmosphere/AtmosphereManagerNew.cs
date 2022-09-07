using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AtmosphereManagerNew : MonoBehaviour
{
    public float dayLength = 100f; // In seconds
    public float startOffset = 0.25f;
    public Material skyMat; // Background sky
    public Transform celestialParent;
    public Transform moon; 
    public Transform sun;
    public Transform stars; 
    public Material[] backgroundMats;
    public Gradient backgroundGradient;
    private Vector4[] skyGradientArray = new Vector4[50]; // Array to be passed on to the sky shader.

    public GlowScript[] glowScripts;

    public float updateFrequency = 15f; // Times per second we update shaders.

    public float tod; // Time of day
    public float atmoIntensity;
    public float fogIntensity;
    public float fogMult;
    public Color origColor;

    private float lastUpdateTime;

    void Start()
    {
        UpdateShadersGlobalAtStart();

    }


    void Update()
    {
        tod = ((Time.time / dayLength) + startOffset) % 1f;
        
        if(Time.time > lastUpdateTime + 1f / updateFrequency) {
            fogIntensity = ((Mathf.Sin((Time.time + 70f) / 20f) + 1f) / 2f) * 0.5f + 0.3f;
            fogIntensity *= fogMult;
            lastUpdateTime = Time.time;
            UpdateFog();
            UpdateScripts();
        }

        RotateCelestials();
    }

    void RotateCelestials () {
        celestialParent.rotation = Quaternion.Euler(0f, 0f, -tod * 360f);
        moon.rotation = Quaternion.Euler(0f, 0f, -celestialParent.rotation.z);
        sun.rotation = Quaternion.Euler(0f, 0f, -celestialParent.rotation.z);
        stars.rotation = Quaternion.Euler(110f + tod * 360f, 90f, -90f);
    }

    void UpdateScripts() {
        foreach(GlowScript glowScript in glowScripts) {
            glowScript.SetTOD(tod);
        }
        Vector2 sunScreenPos = Camera.main.WorldToScreenPoint(sun.position);
        Shader.SetGlobalVector("_SunPos", new Vector3(sunScreenPos.x / (float)Screen.width, sunScreenPos.y / (float)Screen.height, 0f));
    }

    void UpdateShaders() {
        skyMat.SetFloat("_tod", tod);

        AtmosphereIntensityControl();

        foreach(Material mat in backgroundMats) {
            mat.SetColor("_nightColor", backgroundGradient.Evaluate(0f));
            mat.SetColor("_morningColor", backgroundGradient.Evaluate(0.25f));
            mat.SetColor("_dayColor", backgroundGradient.Evaluate(0.5f));
            mat.SetColor("_eveningColor", backgroundGradient.Evaluate(0.75f));
            mat.SetFloat("_atmoIntensity", atmoIntensity);
            mat.SetColor("_origColor", origColor);
        }

        
        skyMat.SetFloat("_fogIntensity", fogIntensity);

        foreach(Material mat in backgroundMats) {
            mat.SetFloat("_tod", tod);
        }
        foreach(Material mat in backgroundMats) {
            mat.SetFloat("_fogIntensity", fogIntensity);
        }

    }

    void GradientToVector4 (int resolution) {
        for(int i = 0; i < resolution; i++) {
            skyGradientArray[i] = backgroundGradient.Evaluate(i / (float)resolution);
        }
    }

    void UpdateShadersGlobalAtStart () {
        Shader.SetGlobalFloat("_DayLength", dayLength);
        Shader.SetGlobalFloat("_DayOffset", startOffset);
        GradientToVector4(50);
        skyMat.SetVectorArray("_SkyGradient", skyGradientArray);
        foreach(Material mat in backgroundMats) {
            mat.SetVectorArray("_SkyGradient", skyGradientArray);
        }
    }
    void UpdateShadersGlobal () {
        Shader.SetGlobalFloat("_TOD", tod);
    }

    void AtmosphereIntensityControl() {
        if(tod >= 0.2f && tod <= 0.5f) {
            atmoIntensity = tod + 0.2f;
        }
        else if(tod > 0.5f && tod <= 0.8f) {
            float oneMinus = 1f - tod;
            atmoIntensity = oneMinus + 0.2f;
        }
        else if(tod > 0.8f && tod <= 0.9f){
            atmoIntensity = 0.4f + 0.85f * (tod - 0.8f) / 0.1f;
        }
        else if(tod > 0.9f) {
            atmoIntensity = 1.25f + 0.15f * (tod - 0.9f) / 0.1f;
        }
        else if(tod <= 0.1f) {
            atmoIntensity = 1.4f - 0.15f * tod / 0.1f;
        }
        else {
            atmoIntensity = 1.25f - 0.85f * (tod - 0.1f) / 0.1f;
        }

        atmoIntensity = Mathf.Lerp(atmoIntensity, 1f, (fogIntensity - 0.35f) / 0.65f);
    }

    void UpdateFog () {
        skyMat.SetFloat("_fogIntensity", fogIntensity);
        foreach(Material mat in backgroundMats) {
            mat.SetFloat("_fogIntensity", fogIntensity);
        }
    }

    void OnValidate() {
        tod = tod % 1f;

        RotateCelestials();

        UpdateFog();
        UpdateScripts();
        UpdateShadersGlobal();
        UpdateShadersGlobalAtStart();
    }
}
