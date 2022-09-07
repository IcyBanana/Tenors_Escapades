using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CinematicTimeChange : MonoBehaviour
{
    public AtmosphereManagerNew atmo;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown("k")) {
            atmo.startOffset += 0.1f;
        }

        if(Input.GetKeyDown("j")) {
            atmo.startOffset -= 0.1f;
        }
    }
}
