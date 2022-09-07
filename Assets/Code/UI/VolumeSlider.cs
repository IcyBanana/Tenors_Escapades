using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeSlider : MonoBehaviour
{
    public string prefsName;
    public Slider mySlider;
    public AudioMixer targetMixer;
    public enum AudioGroup {
        Music,
        Effects,
        Ambient
    }
    public AudioGroup targetGroup;
    

    
    void Start()
    {
        LoadValues(); // Load values from player prefs. If none exist, create a new one with current value.
        mySlider.onValueChanged.AddListener(delegate {CheckValueChange();});
    }

    
    public void CheckValueChange()
    {
        float sliderValue = mySlider.value;
        if(sliderValue < 0.0001f) // Make sure we don't reach 0. (Using log)
            sliderValue = 0.0001f;
        float adjustedValue = Mathf.Log10(mySlider.value) * 20;

        switch (targetGroup) {
            case AudioGroup.Music:
                targetMixer.SetFloat("musicVol", adjustedValue); 
            break;
            case AudioGroup.Effects:
                targetMixer.SetFloat("effectsVol", adjustedValue);
            break;
            case AudioGroup.Ambient:
                targetMixer.SetFloat("ambientVol", adjustedValue);
            break;
        }
    }

    public void SaveValues () { // Called when exiting menu.
        PlayerPrefs.SetFloat(prefsName, mySlider.value);
    }

    void LoadValues () {
        if(PlayerPrefs.HasKey(prefsName)) {
            mySlider.value = PlayerPrefs.GetFloat(prefsName);
        }
        else {
            PlayerPrefs.SetFloat(prefsName, mySlider.value);
        }
    }
}
