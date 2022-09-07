using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScarfScript : MonoBehaviour
{
    private LineRenderer myLine;
    public Transform playerTransform;
    public float speed;
    public float minSpeed;
    public float maxSpeed;
    public int maxSegments = 10;
    public float segmentLength = 0.2f;

    private float sPerSeg;
    private int currentSegments = 0;
    public Vector2[] pastPositions;
    private Vector2 lastPos;

    // Start is called before the first frame update
    public void Initialize()
    {
        myLine = GetComponent<LineRenderer>();
        sPerSeg = (maxSpeed - minSpeed) / (float)maxSegments;
        pastPositions = new Vector2[maxSegments];
    }

    // Update is called once per frame
    void Update()
    {
        if(speed > maxSpeed)
            speed = maxSpeed;
        currentSegments = (int)((speed - minSpeed) / sPerSeg);
        UpdateSegPositions();
        transform.rotation = Quaternion.Euler(0f, 0f, -playerTransform.rotation.z);
    }

    void UpdateSegPositions () {   
        if(Vector2.Distance(transform.position, lastPos) > segmentLength) {
            for(int i = maxSegments - 1; i > 0; i--) {
                pastPositions[i] = pastPositions[i-1];
            }
            pastPositions[0] = lastPos;
            lastPos = transform.position;
            
            if(currentSegments > 0) {
                myLine.positionCount = currentSegments + 1;
                myLine.SetPosition(0, Vector3.zero);
                for(int i = 1; i < currentSegments + 1; i++) {
                    if(i < maxSegments) {
                        myLine.SetPosition(i, pastPositions[i] - (Vector2)transform.position);
                    }
                }
            }
            else {
                myLine.positionCount = 0;
            }   
        }
    }
}
