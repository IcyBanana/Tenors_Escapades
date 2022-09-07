using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialExplain : MonoBehaviour
{
    [Header("Explain Point Properties")]
    public Transform respawnPoint; // Where the player respawns if they fail this 'explain' point.
    public Transform explainFocus; // The focus of this explain point. Could be a rock, could be a terrain feature. This is what the text bubble appears over.
    public bool slowTime = true;
    public bool allowPlayerJump;   // Allows/Forbids player to jump during this explain point.
    public bool disablePlayerJumpExit; // Disables player jump after this explain point.
    public bool allowPlayerFlip;   // Allows/Forbids player to flip during this explain point.
    public bool disablePlayerFlipExit; // Disables player flipping after this explain point.

    public bool allowPlayerJumpExit = false;
    public bool allowPlayerFlipExit = false;

    public bool isLastPoint = false;
    public GameManager gameManager;

    public AudioClip tapSound; // Played when player taps to accept tutorial info.

    private PlayerController playerController;

    private bool playerInside = false;

    private bool playedSound = false;

    [Header("UI")]
    public TutorialBubble textBubble;
    public int stringIndex = 0;

    [Header("Tutorial Helper")]
    public TutorialHelper tutorialHelper;

    void Update () {
        if(!playerInside)
            return;

 
        textBubble.SetRectPos(GetFocusOnScreen());
        
    }

    Vector2 GetFocusOnScreen () { // Returns normalized screen position of our focus transform.
        Vector2 normalizedPos = Vector2.zero;
        if(explainFocus) {
            normalizedPos = Camera.main.WorldToScreenPoint(explainFocus.position + Vector3.up * 3f);
            normalizedPos = new Vector2(normalizedPos.x / Camera.main.pixelWidth, normalizedPos.y / Camera.main.pixelHeight);
        }

        return normalizedPos;
    }

    void OnTriggerEnter2D (Collider2D col) {
        if(col.gameObject.layer == 12) {
            playerInside = true;

            if(slowTime)
                Camera.main.GetComponent<CameraFollow>().StartTimescaleChange(0f, 2f);

            playerController = col.GetComponent<PlayerController>();
            if(allowPlayerJump) {   
                playerController.canJump = true;
            }
            else {
                playerController.canJump = false;
            }   

            if(allowPlayerFlip) {
                playerController.canFlip = true;
            }
            else {
                playerController.canFlip = false;
            }   

            textBubble.ChangeText(stringIndex);
            //textBubble.Appear();   
            tutorialHelper.QueueBubble(this);
            tutorialHelper.lastPoint = stringIndex;     
        }
    }

    void OnTriggerExit2D (Collider2D col) {
        if(col.gameObject.layer == 12) {
            playerInside = false;

            playerController = col.GetComponent<PlayerController>();
            if(disablePlayerJumpExit) {   
                playerController.canJump = false;
            }

            if(disablePlayerFlipExit) {
                playerController.canFlip = false;
            }

            if(allowPlayerJumpExit) {
                playerController.canJump = true;
            }
            if(allowPlayerFlipExit) {
                playerController.canFlip = true;
            }

            textBubble.Hide();

            if(isLastPoint) {
                gameManager.FinishTutorial();
                playerController.AdjustMinSpeed(2f);
            }
        }
    }
}
