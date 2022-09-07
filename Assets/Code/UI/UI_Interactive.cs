using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Interactive : MonoBehaviour
{
    public AnimationCurve moveCurve; // Animation curve for movement. X = 0 is Start position, X = 1 is Destination; Y = 0 is Start time, Y = 1 is End time.
    private RectTransform myRect;
    private Text myText;

    // MOVE
    private float moveStartTime; // Time.time when we last started moving.
    private float moveDur;   // Duration of current move animation.
    private float moveP = 2f;     // Percentage of our position in the current move animation.
    private Vector2 moveStartPos;
    private Vector2 moveDest; // Destination of movement.

    // SCALE
    private float scaleStartTime;
    private float scaleDur;
    private float scaleP = 2f;
    private Vector2 oldScale;
    private Vector2 newScale;

    // COLOR
    private float colorStartTime;
    private float colorDur;
    private float colorP = 2f;
    private Color oldColor;
    private Color newColor;

    // GAME OBJECT
    private float deactivateStart = -1f;
    private float deactivateDelay;

    void Awake()
    {
        myRect = GetComponent<RectTransform>();
        myText = GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        if(moveP < 1f)
            Move();
        if(scaleP < 1f)
            Scale();
        if(colorP < 1f)
            Color();

        if(deactivateStart > 0f && (Time.time - deactivateStart) >= deactivateDelay) {
            deactivateStart = -1f;
            gameObject.SetActive(false);
        }
    }

    // SetMoveDependant is used to move a UI element between two constant positions. Can be called while in motion - this will shorten the new movement 
    // depending on how much we had left to move on the first call.
    public void SetMoveDependant (UIAnim_Move moveData) {
        moveStartTime = Time.time;
        moveDur = moveData.duration * moveP; // Set new move duration relative to how far into the last one we were.
        moveDest = moveData.destination;
        moveStartPos = myRect.anchoredPosition;
        moveP = 0f;

        deactivateStart = -1f;
    }

    public void Deactivate (float delay) {
        deactivateDelay = delay;
        deactivateStart = Time.time;
    }

    public void SetScaleDependant(UIAnim_Scale scaleData) {
        scaleStartTime = Time.time;
        scaleDur = scaleData.duration * scaleP;
        newScale = scaleData.newScale;
        oldScale.x = myText.fontSize;
        scaleP = 0f;

        deactivateStart = -1f;
    }

    public void SetColor(UIAnim_Color colorData) {
        colorStartTime = Time.time;
        colorDur = colorData.duration;
        newColor = colorData.newColor;
        if(myText)
            oldColor = myText.color;
        colorP = 0f; 

        deactivateStart = -1f;
    }

    void Color () {
        if(myText) {
            Color direction = newColor - oldColor;
            colorP = Mathf.Clamp((Time.time - scaleStartTime) / scaleDur, 0f, 1f);
            myText.color = oldColor + direction * moveCurve.Evaluate(colorP);
        }
        else {

        }   
    }

    void Scale () {
        if(myText) {
            int direction = (int)(newScale.x - oldScale.x);
            scaleP = Mathf.Clamp((Time.time - scaleStartTime) / scaleDur, 0f, 1f);
            myText.fontSize = (int)(oldScale.x + direction * moveCurve.Evaluate(scaleP));
        }
        else {

        }   
    }

    void Move () {
        Vector2 direction = moveDest - moveStartPos;
        moveP = Mathf.Clamp((Time.time - moveStartTime) / moveDur, 0f, 1f);
        myRect.anchoredPosition = moveStartPos + direction * moveCurve.Evaluate(moveP);
    }
}
