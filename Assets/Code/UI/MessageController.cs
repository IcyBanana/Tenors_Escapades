using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MessageController : MonoBehaviour
{
    // Message Controller - Assigns actions to the Message Bubble << MessageBubble.cs >>

    private PlayerController playerController;

    [Header("Message Bubble Parameters")]
    public MessageBubble messageBubble;
    public float bubbleClickDelay = 1f;    // Delay in seconds before we can click to dismiss the bubble.
    private bool bubbleVisible = false;
    private bool queuedBubble = false;
    private float bubbleAppearTime = -1f; // Time.time when bubble appeared.
    private bool clickedDismiss = false; // True once we clicked to dismiss.

    private bool diedInChasm = false;


    void Start () {
        playerController = GetComponent<PlayerController>();
    }

    void Update() {
        AttemptShowBubble(); // Attempts to show the bubble. Will show if the player crashed and has TTP (Touch to play) on true.

        if(clickedDismiss && bubbleAppearTime > 0f && Time.unscaledTime >= bubbleAppearTime + bubbleClickDelay) {
            ResetBools();
            messageBubble.Hide();
        }

        if(!bubbleVisible) // We skip adjustments if the bubble isn't shown yet.
            return;
        
        AdjustBubble(); // Adjusts bubble position and hides it if the player has clicked.
    }


    public void Crashed (bool chasm) {
        if(chasm) {
            diedInChasm = true;
            queuedBubble = true;
        }
        else { 
            queuedBubble = true;
        }
    }

    void AttemptShowBubble () {
        if(playerController.getTTP() && queuedBubble) {
            if(!bubbleVisible) {
                bubbleVisible = true;
                bubbleAppearTime = Time.time;
            }
            if(diedInChasm) {
                messageBubble.ChangeText(1);
                messageBubble.Appear(); 
            }
            else {
                messageBubble.ChangeText(0);
                messageBubble.Appear(); 
            }
        }
    }
    
    void AdjustBubble () {
        if(Input.GetMouseButtonDown(0)) {
            clickedDismiss = true;
        }   
        else {
            messageBubble.SetRectPos(GetFocusOnScreen());
        }
    }

    Vector2 GetFocusOnScreen () { // Returns normalized screen position of our player transform.
        Vector2 normalizedPos = Vector2.zero;
        
        normalizedPos = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 3f);
        normalizedPos = new Vector2(normalizedPos.x / Camera.main.pixelWidth, normalizedPos.y / Camera.main.pixelHeight);
        
        if(diedInChasm) {
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
        diedInChasm = false;
        queuedBubble = false;
    }
}
