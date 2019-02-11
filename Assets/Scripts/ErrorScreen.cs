using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ErrorScreen : MonoBehaviour {

    public GameObject errorScreen;

    public void openError()
    {
        errorScreen.SetActive(true);
    }

    public void returnButton()
    {
        SceneManager.LoadScene(0);
    }

}
