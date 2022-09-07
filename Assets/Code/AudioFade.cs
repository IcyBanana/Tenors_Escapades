using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioFade : MonoBehaviour
{
    // Makes audio source fade in and out at the flip of a switch (isOn).
    
    public float lerpSpeed = 5f;
    private AudioSource myAudio;
    public bool isOn = true;

    void Start () {
        myAudio = GetComponent<AudioSource>();
        if(isOn) {
            myAudio.volume = 1f;
        }
        else {
            myAudio.volume = 0f;
        }
    }   

    void Update () {
        if(isOn) {
            myAudio.volume = Mathf.Lerp(myAudio.volume, 1f, lerpSpeed * Time.unscaledDeltaTime);
        }
        else {
            myAudio.volume = Mathf.Lerp(myAudio.volume, 0f, lerpSpeed * Time.unscaledDeltaTime);
        }
    }

    public void FadeOut () {
        isOn = false;
    }

    public void FadeIn () {
        isOn = true;
    }

}
