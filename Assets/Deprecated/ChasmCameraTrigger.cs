using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChasmCameraTrigger : MonoBehaviour
{
    // During chasm jumps camera receives a new target from this code. The target is the chasm center. The camera then adjusts to follow a floating point between the chasm and the player.

    public Transform newTarget; // Chasm center to give to camera.
    public float midPoint = 0.8f; // How far are we along the Player>Chasm vector we are.

    public LayerMask triggerLayer;

    public float xSize = 10f; // How far we extend before and after the chasm.

    public Transform chasmFog;

    private CameraFollow cameraFollow;
    private float chasmSize; // Size of our controlled chasm.
    private Transform playerTrans;


    // Update is called once per frame
    void Update()
    {
        /*if(playerTrans.position.x >= transform.position.x + (chasmSize + xSize) / 2f) {
            cameraFollow.SwitchToPlayer();
        }*/
    }
    /*void OnTriggerEnter(Collider col) {
        if(!enabled)
            return;
        print("triggered but not recog");
        if((triggerLayer.value & (1 << col.transform.gameObject.layer)) > 0) {
            print("triggered");
            cameraFollow.SwitchToChasm(newTarget, midPoint);
            playerTrans = cameraFollow.target;
        }
    }
    /*void OnTriggerExit(Collider col) {
        if((triggerLayer.value & (1 << col.transform.gameObject.layer)) > 0) {
            mainCamera.GetComponent<CameraFollow>().SwitchToPlayer();
        }
    }*/

    public void Initialize (float size) {
        chasmSize = size;
        GetComponent<BoxCollider>().size = new Vector3(chasmSize + xSize * 2f, 20f, 90f); 
        //newTarget.position += Vector3.up * size / 3f;
        cameraFollow = Camera.main.GetComponent<CameraFollow>();
        playerTrans = GameObject.Find("Player").transform;
        chasmFog.localScale = new Vector3(size / 5f, 3f, 1f);
    }

    public void SetChasmSize (float size) {
        chasmSize = size;
    }
}
