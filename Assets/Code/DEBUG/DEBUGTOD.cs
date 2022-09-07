using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DEBUGTOD : MonoBehaviour
{
    public AtmosphereManagerNew atmo;
    public Transform playerTransform;
    public int timesRestarted = 0;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        GetComponent<UnityEngine.UI.Text>().text = "TOD: " + atmo.tod + " PlayerX: " + playerTransform.position.x + " PlayerY: " + playerTransform.position.y + " Restarts: " + timesRestarted;
    }
}
