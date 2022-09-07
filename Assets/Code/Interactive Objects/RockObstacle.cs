using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RockObstacle : MonoBehaviour
{
    public float distanceToDestroy = 50f; // When the player passes the obstacle this much on the X axis, the object gets destroyed.
    private Transform playerTransform;

    public MeshRenderer artMeshRenderer; // Mesh renderer of the graphic component.

    public bool willBounce = false; // If this is true, the player will bounce off the rock instead of crashing.
    public float bounceHeight = 2f;

    public RockCodedAnimation animationCode;

    public AudioClip rockSmashAudio;
    public AudioClip rockBounceAudio;
    public AudioSource myAudioSource;

    private bool hasTriggered = false;

    // Start is called before the first frame update
    void Start()
    {
        playerTransform = GameObject.Find("Player").transform;
    }

    // Update is called once per frame
    void Update()
    {
        if(playerTransform.position.x > transform.position.x + distanceToDestroy) {
            GameObject.Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D col) {
        if(hasTriggered) // We already triggered our intended interaction.
            return;
        
        // Layer 12 is player layer
        if(col.gameObject.layer == 12) {
            PlayerController playerController = playerTransform.GetComponent<PlayerController>();
            if(playerController.hasCrashed)
                return;
            
            // If player hits the rock while airborne and falling (yV < 0)
            if(!playerController.isGrounded && playerController.playerRigidbody.velocity.y < 0f)
                willBounce = true;
            

            // Bounce
            if(willBounce) {
                Rigidbody2D playerRigidbody = playerTransform.GetComponent<Rigidbody2D>();
                Vector2 forceToAdd = Vector2.up * Mathf.Sqrt((playerRigidbody.gravityScale * Physics.gravity.magnitude) * 2f * bounceHeight) * playerRigidbody.mass;
                playerRigidbody.velocity -= Vector2.up * playerRigidbody.velocity.y;
                playerRigidbody.AddForce(forceToAdd, ForceMode2D.Impulse);
                GameObject.Find("GameManager").GetComponent<TrickSystem>().AddTrick(TrickSystem.TrickType.RockBounce);
                playerController.jumpLandAudioSource.PlayOneShot(rockBounceAudio);
                hasTriggered = true;
            }
            //Crash or explode
            else {
                if(playerController.CheckInvincibility()) {
                    if(!willBounce && !hasTriggered)
                        Explode();
                }
                else {
                    Rigidbody2D playerRigidbody = playerTransform.GetComponent<Rigidbody2D>();
                    Vector2 forceToAdd = Vector2.up * Mathf.Sqrt((playerRigidbody.gravityScale * Physics.gravity.magnitude) * 2f * 1f) * playerRigidbody.mass;
                    playerRigidbody.AddForce(forceToAdd, ForceMode2D.Impulse);
                    playerController.Crash(true);
                }
            }
        }
    }

    void Explode () {
        GameObject.Find("GameManager").GetComponent<TrickSystem>().AddTrick(TrickSystem.TrickType.RockSmash);
        playerTransform.GetComponent<PlayerController>().jumpLandAudioSource.PlayOneShot(rockSmashAudio);
        hasTriggered = true;
        animationCode.Animate();
        artMeshRenderer.enabled = false;
    }
}
