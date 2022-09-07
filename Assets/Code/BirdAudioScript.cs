using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BirdAudioScript : MonoBehaviour
{
    // Makes bird sounds and moves attached transform around using sine functions.
    
    public AudioSource audioSource;
    public AudioClip[] birdClips;

    public Vector2 timeIntervals = new Vector2(5f, 15f);
    private float nextInterval;
    private float lastTime;


    void Start()
    {
        nextInterval = Random.Range(timeIntervals.x, timeIntervals.y);
    }


    void Update()
    {
        MoveAround();
    }

    void MoveAround () {
        if(Time.time > lastTime + nextInterval) {
            lastTime = Time.time;
            nextInterval = Random.Range(timeIntervals.x, timeIntervals.y);
            transform.position = new Vector3(Mathf.Sin(Time.time / 3f) * 40f, 15f + Mathf.Sin(Time.time / 2f) * 15f, transform.position.z);
            int i = Random.Range(0, birdClips.Length);
            audioSource.pitch = 1f + Random.Range(-0.08f, 0.12f);
            audioSource.PlayOneShot(birdClips[i]);
        }
    }
}
