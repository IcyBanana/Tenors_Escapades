using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsButton : MonoBehaviour
{
    public GameObject[] settingButtons;

    public void OnPress () { // Called from the Main Menu UI on button press.
        foreach(GameObject button in settingButtons) {
            button.SetActive(true);
        }
    }
}
