using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CountdownScript : MonoBehaviour {

    public Text countdownText;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    //Simple Countdown before the movement of the map starts
    public IEnumerator countdown(int time)
    {
        int i = time;
        while(i != 0)
        {
            countdownText.text = "" + i;
            yield return new WaitForSeconds(1.0f);
            i--;
        }
        NoteSheetGen.start = true;
        this.gameObject.SetActive(false);
    }

}
