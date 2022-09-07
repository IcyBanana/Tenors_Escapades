using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlagScript : MonoBehaviour
{
    
    public float acceleration = -2f;
    public float accelRandom = 0.5f; // Randomize acceleration by up to this much in either direction.

    private Transform playerTransform;
    private PlayerController playerController;
    private float currentSpeed;
    private float randomRotation;
    
    private bool isFalling = false;
    

    void Start () {
        playerTransform = GameObject.Find("Player").transform;
        playerController = playerTransform.GetComponent<PlayerController>();

        acceleration += Random.Range(-accelRandom, accelRandom);
        randomRotation = Random.Range(-80f, 80f);
    }

    void FixedUpdate () {
        if(isFalling) {
            transform.position += Vector3.up * currentSpeed * Time.fixedDeltaTime;
            currentSpeed += acceleration * Time.fixedDeltaTime;
            transform.Rotate(Vector3.forward * randomRotation * Time.fixedDeltaTime);
        }
        else {
            if(playerTransform.position.x >= transform.position.x) {
                float distance = Vector3.Distance(transform.position, playerTransform.position);
                if(distance <= 1f && playerController.isGrinding)
                    isFalling = true;
            }
        }
        DestroyWhenFar();
    }

    void DestroyWhenFar () {
        if(playerTransform.position.x > transform.position.x + 50f)
            GameObject.Destroy(gameObject);
    }
}
