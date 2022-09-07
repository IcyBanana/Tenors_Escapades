using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShaderXWidth : MonoBehaviour
{
    //public Shader snowShader;
    private Camera myCamera;
    public float xWidth; // The distance in units from the left edge of the screen to the right at Z = 0.

    public GameManager gameManager; // Game manager used for determining the next chasm position. We use this to decide if we should update global shader xWidth.

    private bool passedEnd = false;
    private float endX;

    void Start()
    {
        myCamera = GetComponent<Camera>();
        endX = gameManager.currentChunk.GetStartEnd().y;
    }

    // Update is called once per frame
    void Update()
    {
        if(passedEnd) {
            endX = gameManager.currentChunk.GetStartEnd().y;
            passedEnd = false;
        }

        if(transform.position.x > endX + 50f)
            passedEnd = true;

        if(transform.position.x > endX - 50f && transform.position.x < endX + 50f) {
            xWidth = CalculateXWidth();
            Shader.SetGlobalFloat("_XWidth", xWidth);
        }
        
    }

    float CalculateXWidth () {
        float camZ = -transform.position.z;
        float camFOVRad = myCamera.fieldOfView;
        camFOVRad = Camera.VerticalToHorizontalFieldOfView(camFOVRad, Screen.width / (float)Screen.height) * Mathf.PI / 180f;

        // xWidth is half of the X distance between edges of the screen at z = 0.
        float xWidth = (camZ / Mathf.Cos(camFOVRad / 2f) * Mathf.Sin(camFOVRad / 2f));

        return (xWidth);
    }

    public float CalculateXWidth (float fov) {
        float camZ = -transform.position.z;
        float camFOVRad = fov;
        camFOVRad = Camera.VerticalToHorizontalFieldOfView(camFOVRad, Screen.width / (float)Screen.height) * Mathf.PI / 180f;

        // xWidth is half of the X distance between horizontal edges of the screen at z = 0.
        float xWidth = (camZ / Mathf.Cos(camFOVRad / 2f) * Mathf.Sin(camFOVRad / 2f));

        return (xWidth);
    }

    public float CalculateYWidth (float fov) {
        float camZ = -transform.position.z;
        float camFOVRad = fov * Mathf.PI / 180f;

        // yWidth is half of the Y distance between vertical edges of the screen at z = 0.
        float yWidth = (camZ / Mathf.Cos(camFOVRad / 2f) * Mathf.Sin(camFOVRad / 2f));

        return (yWidth);
    }
}
