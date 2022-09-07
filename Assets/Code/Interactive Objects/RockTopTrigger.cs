using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RockTopTrigger : MonoBehaviour
{
    public RockObstacle mainRock;
    void OnTriggerEnter2D(Collider2D col) {
        if(col.gameObject.layer == 12) {
            mainRock.willBounce = true;
        }
    }
}
