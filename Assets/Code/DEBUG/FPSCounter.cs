using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSCounter : MonoBehaviour
{
    private float highestFrameTime = 0f;
    private float lastFrameTime;
    private float currentFrameTime;

    private float totalAverage;
    private float totalSum;
    private int totalFramesCounted;
    
    void Start()
    {
        
    }

    
    void Update()
    {
        currentFrameTime = Time.unscaledDeltaTime;
        if(currentFrameTime > highestFrameTime && currentFrameTime < 60f) {
            highestFrameTime = currentFrameTime;
        }
        totalFramesCounted++;
        totalSum += currentFrameTime;
        totalAverage = totalSum / (float)totalFramesCounted;

        lastFrameTime = currentFrameTime;
    }

    void OnGUI () {
        GUI.skin.textField.fontSize = 30;
        GUI.TextField(new Rect(Screen.width / 2f - 300f, 200f, 800f, 200f), "FPS: " + (1f/currentFrameTime) + " | Lowest: " + (1f/highestFrameTime) + " | Avg: " + (1f/totalAverage));
        if(GUI.Button(new Rect(Screen.width / 2f - 300f, 500f, 600f, 100f), "Reset")) {
            highestFrameTime = 0f;
            lastFrameTime = 0f;
            currentFrameTime = 0f;
            totalAverage = 0f;
            totalSum = 0f;
            totalFramesCounted = 0;
        }
    }
}
