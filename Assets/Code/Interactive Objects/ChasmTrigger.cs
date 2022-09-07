using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChasmTrigger : MonoBehaviour
{


    public float xSize = 10f; // How far we extend before and after the chasm.
    public float yOffset = -5f;

    public Transform chasmFog;

    private CameraFollow cameraFollow;
    [SerializeField]
    private float chasmSize; // Size of our controlled chasm.



    private bool hasTriggered = false;



    void OnTriggerEnter2D (Collider2D col) {
        if(hasTriggered)
            return;
        if(col.gameObject.layer == 12) {
            col.GetComponent<PlayerController>().DiedInChasm();
            hasTriggered = true;
        }
    }

    public void Initialize (float size, TerrainChunkBuilder chunkBuilder) {
        chasmSize = size;
        PolygonCollider2D polyCol = GetComponent<PolygonCollider2D>();
        float xPos_0 = -xSize - (chasmSize / 2f);
        float xPos_1 = (chasmSize / 2f);
        Vector2[] newPoints = new Vector2[4];
        newPoints[0] = new Vector2(xPos_0, -10f);
        newPoints[1] = new Vector2(xPos_1, 0f);
        newPoints[3] = polyCol.points[1] - Vector2.up * 10f;
        newPoints[2] = polyCol.points[0] - Vector2.up * 10f;
        polyCol.points = newPoints;
        //newTarget.position += Vector3.up * size / 3f;
        cameraFollow = Camera.main.GetComponent<CameraFollow>();
        chasmFog.localScale = new Vector3(size / 5f, 3f, 1f);
    }
}
