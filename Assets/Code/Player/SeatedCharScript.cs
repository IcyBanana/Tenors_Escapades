using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeatedCharScript : MonoBehaviour
{
    public GameObject myGraphic;

    public void StartGame() {
        myGraphic.SetActive(false);
    }
}
