using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForegroundGlow : MonoBehaviour
{
    public Transform sun;
    public Camera foreCam;

    public float dist = 15f;

    public bool constantZ;
    public float z;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 camToSun = (sun.position - foreCam.transform.position).normalized;
        if(constantZ) {
            dist = z / Mathf.Cos(Vector3.Angle(camToSun, foreCam.transform.forward) / 180f * Mathf.PI);
            transform.position = camToSun * dist + foreCam.transform.position;
        }
        else {
            transform.position = camToSun * dist + foreCam.transform.position;
        }
        
    }
}
