using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RockCodedAnimation : MonoBehaviour
{
    
    /*
        Rock Coded Animation: I'll do it myself...
    */

    [Header("Timing")]
    public float length = 1.5f; // Length in seconds.
    private float startTime;    // When did we start animating.
    public float refreshRate = 15f; // How many times per second we animate the pieces.
    private float lastRefresh = -1f;

    [Header("Vectors")]
    public Vector2 biasVector = new Vector2(1f, 0.2f).normalized; // Bias for motion.
    public Vector2 biasRange = new Vector2(0.2f, 0.6f); // How much the bias can affect a piece. X - min, Y - max. Selected at random per piece.

    public Vector2 velMinMax = new Vector2(0.5f, 2f); // Min and max velocities possible for motion of pieces.
    public Vector2 angVelMinMax = new Vector2(25f, 180f); // Min and max velocities possible for rotation of pieces. Deg/sec.

    private int pieceCount;
    public Transform[] pieces;
    private float[] biasAmount;
    private float[] angularVel; // Angular velocity for each piece.

    private Vector2[] finalVectors; // Final motion vectors per piece.

    private bool isReady = false;
    
    void Start() {
        pieceCount = pieces.Length;
        InitializeArrays();
        GenerateBiasAmount();
        GenerateMotionVector(); 
    }

    public void Animate () { 
        foreach(Transform trans in pieces) {
            trans.gameObject.SetActive(true);
        }
        startTime = Time.time;
        isReady = true;
    }


    // Update is called once per frame
    void Update()
    {
        if(!isReady)
            return;

        if(Time.time - lastRefresh > 1f / refreshRate) {
            MovePieces();
        }
        if(Time.time - startTime > length) {
            GameObject.Destroy(transform.parent.gameObject);
        }
    }

    void MovePieces () {
        for(int i = 0; i < pieceCount; i++) {
            float dampMult = 1f - Mathf.Clamp01((Time.time - startTime) / length);
            pieces[i].position += (Vector3)(finalVectors[i] / refreshRate) * dampMult;
            pieces[i].Rotate(new Vector3(0f, 0f, angularVel[i]) / refreshRate * dampMult);
        }
    }

    void GenerateMotionVector () { // Also assigns angular velocities..
        for(int i = 0; i < pieceCount; i++) {
            // Generate random direction with upwards bias. (explosion)
            Vector2 direction = (Vector2.up + Vector2.right * Random.Range(-6f, 6f)).normalized;

            // Add forward bias based on biasAmount
            direction = (direction + (biasVector * biasAmount[i])).normalized * Random.Range(velMinMax.x, velMinMax.y);

            finalVectors[i] = direction;
            angularVel[i] = Random.Range(angVelMinMax.x, angVelMinMax.y);
        }
    }   

    void GenerateBiasAmount () {
        for(int i = 0; i < pieceCount; i++) {
            biasAmount[i] = Random.Range(biasRange.x, biasRange.y);
            i++;
        }
    }

    void InitializeArrays() {
        biasAmount = new float[pieceCount];
        angularVel = new float[pieceCount];
        finalVectors = new Vector2[pieceCount];
    }
}
