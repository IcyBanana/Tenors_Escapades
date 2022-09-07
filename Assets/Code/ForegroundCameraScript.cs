using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForegroundCameraScript : MonoBehaviour
{
    public Camera mainCamera;
    private Camera myCamera;

    // Start is called before the first frame update
    void Start()
    {
        myCamera = GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        myCamera.fieldOfView = mainCamera.fieldOfView;
    }
}
