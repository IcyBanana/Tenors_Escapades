using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ResetTutorial : MonoBehaviour
{
    public void AttemptTutorialReset () {
        if(PlayerPrefs.HasKey("Tutorial")) {
            if(PlayerPrefs.GetInt("Tutorial") > 0) {
                PlayerPrefs.SetInt("Tutorial", 0);
                Invoke("RestartScene", 1.5f);
            }
        }


    }

    void RestartScene () {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
