using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrameStutter : MonoBehaviour
{

    public int minFrameCount = 50;
    public int maxFrameCount = 150;

    public float targetFPS = 60;
    public float maxFPSMod = 0.75f; // Maximum multiplier of target fps
    public float power = 2;

    private float currentFPSMod = 1;

    private int framesTillStutter;
    private int frameCount = 0; // Since last stutter

    // Start is called before the first frame update
    void Start()
    {
        framesTillStutter = Random.Range(minFrameCount, maxFrameCount);
    }

    // Update is called once per frame
    void Update()
    {
        frameCount++;
        
        if(frameCount > framesTillStutter) {
            currentFPSMod = 1f - (1f - maxFPSMod) * (1f - Mathf.Pow(Random.Range(0f, 1f), 2));
            Application.targetFrameRate = (int)(targetFPS * currentFPSMod);
            frameCount = 0;
            framesTillStutter = Random.Range(minFrameCount, maxFrameCount);
        }
        else if(frameCount == 4) {
            Application.targetFrameRate = (int)(targetFPS);
        }
        
    }
}
