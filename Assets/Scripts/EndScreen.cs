using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class EndScreen : MonoBehaviour {

    public Text scoreText;
    

	// Use this for initialization
	void Start () {
        //Show the score
        scoreText.text = "Correctness: " + PlayerPrefs.GetInt("Score")+"%";
	}
	
    public void returnButton()
    {
        //Restart the game
        SceneManager.LoadScene(0);

    }

}
