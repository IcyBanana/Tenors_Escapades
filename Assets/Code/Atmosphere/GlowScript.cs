using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlowScript : MonoBehaviour
{
    public Vector2 todStart; // Time of day at which we start glowing. X -> We start increasing value, Y -> We reached max value. (0 to 1).
    public Vector2 todStop; // Time of day we stop glowing. X -> Max value, start decreasing. Y -> Value reaches 0. (0 to 1).
    
    public float maxValue;
    public float minValue;

    public bool isMoon = false;

    public bool dynamicColoring = false; // When true we change color based on time of day.
    public Gradient colorGradient;

    private float tod; // Time of day. (0 to 1).
    public bool isSprite = false;
    public SpriteRenderer mySprite;

    public void SetTOD (float tod) {
        this.tod = tod;
        float value = 0f;
        if(isMoon) {
                if(tod >= todStart.x && tod <= todStart.y) {
                    value = minValue + (maxValue - minValue) * (tod - todStart.x) / (todStart.y - todStart.x);
                }
                else if(tod >= todStop.x && tod <= todStop.y) {
                    value = maxValue - (maxValue - minValue) * (tod - todStop.x) / (todStop.y - todStop.x);
                }
                else if(tod > todStart.y && tod <= 1f) {
                    value = maxValue;
                }
                else if(tod >= 0f && tod < todStop.x) {
                    value = maxValue;
                }
                else {
                    value = minValue;
                }
            }
            else {
                if(tod >= todStart.x && tod <= todStart.y) {
                    value = minValue + (maxValue - minValue) * (tod - todStart.x) / (todStart.y - todStart.x);
                }
                else if (tod > todStart.y && tod < todStop.x) {
                        value = maxValue;
                }
                else if(tod >= todStop.x && tod <= todStop.y) {
                    value = maxValue - (maxValue - minValue) * (tod - todStop.x) / (todStop.y - todStop.x);
                }
                else {
                        value = minValue;
                }
            }
        
        if(Application.isPlaying) {
            if(!isSprite) {
                Material myMat = GetComponent<MeshRenderer>().material;
            
                if(dynamicColoring)
                    myMat.color = colorGradient.Evaluate(tod);
                myMat.color = new Color(myMat.color.r, myMat.color.g, myMat.color.b, value);
            }
            else {
                mySprite.color = new Color(0f, 0f, 0f, Mathf.Clamp(value, 0f, maxValue)); 
                if(dynamicColoring)
                    mySprite.color = colorGradient.Evaluate(tod);            
            }
        }
    }
}
