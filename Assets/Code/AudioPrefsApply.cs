using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioPrefsApply : MonoBehaviour
{
    // Applies audio preferences on game load.
    public AudioMixer mainMixer;
    
    void Start()
    {
        float musicValue = Mathf.Log10(PlayerPrefs.GetFloat("MusicVol")) * 20;
        float effectsValue = Mathf.Log10(PlayerPrefs.GetFloat("EffectsVol")) * 20;
        float ambientValue = Mathf.Log10(PlayerPrefs.GetFloat("AmbientVol")) * 20;

        mainMixer.SetFloat("musicVol", musicValue);
        mainMixer.SetFloat("effectsVol", effectsValue);
        mainMixer.SetFloat("ambientVol", ambientValue);
    }


}
