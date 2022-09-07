using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialHelper : MonoBehaviour
{

    // This class is used from events not triggered by zones in the tutorial. Such as player crashing during tutorial.
    public int lastPoint = -1; // Last explain point we've been to.
    public TutorialBubble textBubble;
    public float bubbleClickDelay = 1f;    // Delay in seconds before we can click to dismiss the bubble.
    public AudioClip tapSound;


    private bool bubbleVisible = false;
    private bool queuedBubble = false;
    private float bubbleAppearTime = -1f; // Time.time when bubble appeared.
    private bool clickedDismiss = false; // True once we clicked to dismiss.
    private int textIndex = 0;
    private bool playedSound = false;

    private TutorialExplain tutorialExplain;

    private PlayerController playerController;

    void Start() {
        playerController = GetComponent<PlayerController>();
    }

    void Update() {
        AttemptShowBubble(); // Attempts to show the bubble. Will show if the player crashed and has TTP (Touch to play) on true.

        if(clickedDismiss && bubbleAppearTime > 0f && Time.unscaledTime >= bubbleAppearTime + bubbleClickDelay) {
            ResetBools();

            if(!playedSound) {
                playerController.jumpLandAudioSource.PlayOneShot(tapSound);
                playedSound = true;
            }

            Camera.main.GetComponent<CameraFollow>().TutorialExplainFinished();
            tutorialExplain.enabled = false;
            textBubble.Hide();
        }

        if(!bubbleVisible) // We skip adjustments if the bubble isn't shown yet.
            return;
        
        AdjustBubble(); // Adjusts bubble position and hides it if the player has clicked.
       /* if(!bubbleVisible)
            return;

        
        if(Input.GetMouseButtonDown(0)) {
            textBubble.Hide();
            bubbleVisible = false;
        }   
        else {
            textBubble.SetRectPos(GetFocusOnScreen());
        }*/
    }

    void AdjustBubble () {
        if(Input.GetMouseButtonDown(0)) {
            clickedDismiss = true;
        }   
    }

    void AttemptShowBubble () {
        if(playerController.getTTP() && queuedBubble) {
            if(!bubbleVisible) {
                bubbleVisible = true;
                bubbleAppearTime = Time.unscaledTime;
            }
            textBubble.Appear();    
        }
    }

    public void Crashed () {
        if(lastPoint > 1 && lastPoint < 4) { // Happened between flip point and rope point (Inclusive).
            textBubble.ChangeText(8);
            textBubble.Appear();  
            queuedBubble = true;
        }
        else if(lastPoint > 5 && lastPoint < 7) { // Happened after the chasm point, but before the finish point.
            textBubble.ChangeText(9);
            textBubble.Appear();  
            queuedBubble = true;
        }
    }
    public void QueueBubble (TutorialExplain tutorialExplain) {
        queuedBubble = true;
        this.tutorialExplain = tutorialExplain;
    }

    Vector2 GetFocusOnScreen () { // Returns normalized screen position of our player transform.
        Vector2 normalizedPos = Vector2.zero;
        
        normalizedPos = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 3f);
        normalizedPos = new Vector2(normalizedPos.x / Camera.main.pixelWidth, normalizedPos.y / Camera.main.pixelHeight);
        
        if(lastPoint > 5 && lastPoint < 7) {
            if(normalizedPos.y < 0.5f)
                normalizedPos.y = 0.5f;
            if(normalizedPos.x > 0.5f)
                normalizedPos.x = 0.5f;
        }

        return normalizedPos;
    }

    void ResetBools () {
        bubbleVisible = false;
        clickedDismiss = false;
        bubbleAppearTime = -1f;
        queuedBubble = false;
        playedSound = false;
    }
}
