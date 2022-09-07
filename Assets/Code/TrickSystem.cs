using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrickSystem : MonoBehaviour
{
    public PlayerController playerController;
    public float totalScore = 0f;
    public float chainScore = 0f;
    public int chainMult = 0;
    public TrickUI trickUI;

    public enum TrickType {
        ArchGrind,
        RopeGrind,
        Backflip,
        RockSmash,
        ChasmJump,
        RockBounce
    }
    
    private bool queuedChainFinish; // Chain finish queue for chains started while grounded.
    private float queuedChainFinishDuration = 1f;
    private float queuedChainFinishStartTime;

    private float speedReward;

    private int backflipCount; // Consecutive backflips done back to back. Backflip - Double backflip - Triple backflip - Quadruple backflip.
    private int grindID; // ID of player's current grind object.


    [Header("Trick Rewards - Points")]
    public float[] backflips = new float[5]; // Scores for backflips, each element after the first is for that many consecutive backflips.
    public float archGrindStart = 25f;
    public float ropeGrindStart = 20f;
    public float archGrindPerDistance = 1f; // Score pts per unit of distance traveled.
    public float ropeGrindPerDistance = 0.8f;
    public float rockSmashPoints = 25f;
    public float chasmJumpPoints = 100f;
    public float rockBouncePoints = 25f;

    [Header("Trick Rewards - Speed")]
    public float[] backflipSR = new float[5];
    public float archGrindSR;
    public float ropeGrindSR;
    public float rockSmashSpeed = 0f;
    public float chasmJumpSpeed = 1f;
    public float rockBounceSpeed = 0.5f;

    [Header("Audio")]
    public AudioClip A_trickSuccess;
    public AudioClip A_chainComplete;

    private AudioSource myAudioSource;



    // Start is called before the first frame update
    void Start()
    {
        myAudioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if(playerController.isGrounded && chainMult > 0 && !queuedChainFinish) {
            queuedChainFinish = true;
            queuedChainFinishStartTime = Time.time;
        }
        if(queuedChainFinish && Time.time > queuedChainFinishStartTime + queuedChainFinishDuration) {
            FinishChain();
            queuedChainFinish = false;
        }
    }

    public void FinishChain () {
        if(chainMult > 0)
            myAudioSource.PlayOneShot(A_chainComplete);
        totalScore += chainScore * chainMult;
        backflipCount = 0;
        chainMult = 0;
        chainScore = 0f;
        playerController.AdjustMinSpeed(speedReward);
        speedReward = 0f;
        trickUI.ClearTricks();      
        trickUI.UpdateScore(totalScore);
        
    }

    public void AddTrick (TrickType type) {
        bool finishChainOnEnd = false;
        switch (type) {
            case TrickType.Backflip:
                if(backflipCount == 0) {
                    chainScore += backflips[0];
                    speedReward += backflipSR[0];
                    trickUI.AddTrick("Backflip");
                    myAudioSource.pitch = Random.Range(0.9f, 1.1f);
                    myAudioSource.PlayOneShot(A_trickSuccess);
                }
                else if(backflipCount == 1) {
                    chainScore += backflips[1];
                    speedReward += backflipSR[1];
                    trickUI.AddTrick("Double Backflip");
                    myAudioSource.pitch = Random.Range(0.9f, 1.1f);
                    myAudioSource.PlayOneShot(A_trickSuccess);
                }
                else if(backflipCount == 2) {
                    chainScore += backflips[2];
                    speedReward += backflipSR[2];
                    trickUI.AddTrick("Triple Backflip");
                    myAudioSource.pitch = Random.Range(0.9f, 1.1f);
                    myAudioSource.PlayOneShot(A_trickSuccess);
                }
                else if(backflipCount == 3) {
                    chainScore += backflips[3];
                    speedReward += backflipSR[3];
                    trickUI.AddTrick("Quadruple Backflip");
                    myAudioSource.pitch = Random.Range(0.9f, 1.1f);
                    myAudioSource.PlayOneShot(A_trickSuccess);
                }
                else {
                    chainScore += backflips[4];
                    speedReward += backflipSR[4];
                    trickUI.AddTrick("Insane Backflip");
                    myAudioSource.pitch = Random.Range(0.9f, 1.1f);
                    myAudioSource.PlayOneShot(A_trickSuccess);
                }
                chainMult++;
                backflipCount++;
                break;
            case TrickType.ArchGrind:
                chainMult++;
                backflipCount = 0;
                break;
            case TrickType.RopeGrind:
                chainMult++;
                backflipCount = 0;
                break;
            case TrickType.RockSmash:
                chainMult++;
                backflipCount = 0;
                chainScore += rockSmashPoints;
                speedReward += rockSmashSpeed;
                trickUI.AddTrick("Rock Smash");
                finishChainOnEnd = true;
                break;
            case TrickType.ChasmJump:
                chainMult++;
                backflipCount = 0;
                chainScore += chasmJumpPoints;
                speedReward += chasmJumpSpeed;
                trickUI.AddTrick("Chasm Jump");
                break;
            case TrickType.RockBounce:
                chainMult++;
                chainScore += rockBouncePoints;
                speedReward += rockBounceSpeed;
                trickUI.AddTrick("Rock Bounce");
                break;
        }
        trickUI.UpdateChain(chainScore, chainMult);
        if(finishChainOnEnd)
            FinishChain();        
    } 
    public void AddTrick (TrickType type, int id, float x) { // Current use: grinds... type is trick type; id is instanceID of grind gameobject; x is distance traveled on the grind.
        switch (type) {
            case TrickType.Backflip:
                if(backflipCount == 0) {
                    chainScore += backflips[0];
                    speedReward += backflipSR[0];
                    trickUI.AddTrick("Backflip");
                }
                else if(backflipCount == 1) {
                    chainScore += backflips[1];
                    speedReward += backflipSR[1];
                    trickUI.AddTrick("Double Backflip");
                }
                else if(backflipCount == 2) {
                    chainScore += backflips[2];
                    speedReward += backflipSR[2];
                    trickUI.AddTrick("Triple Backflip");
                }
                else if(backflipCount == 3) {
                    chainScore += backflips[3];
                    speedReward += backflipSR[3];
                    trickUI.AddTrick("Quadruple Backflip");
                }
                else {
                    chainScore += backflips[4];
                    speedReward += backflipSR[4];
                    trickUI.AddTrick("Insane Backflip");
                }
                chainMult++;
                backflipCount++;
                break;
            case TrickType.ArchGrind:
                if(id != grindID) {
                    grindID = id;
                    chainMult++;
                    chainScore += archGrindStart;
                    speedReward += archGrindSR;
                    trickUI.AddTrick("Arch Grind");
                }
                else {
                    chainScore += archGrindPerDistance * x;
                }
                backflipCount = 0;
                break;
            case TrickType.RopeGrind:
                if(id != grindID) {
                    grindID = id;
                    chainMult++;
                    chainScore += ropeGrindStart;
                    speedReward += ropeGrindSR;
                    trickUI.AddTrick("Rope Grind");
                    myAudioSource.pitch = Random.Range(0.9f, 1.1f);
                    myAudioSource.PlayOneShot(A_trickSuccess);
                }
                else {
                    chainScore += ropeGrindPerDistance * x;
                }
                backflipCount = 0;
                break;
        }
        trickUI.UpdateChain(chainScore, chainMult);
        
    } 
}
