using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderValue : MonoBehaviour {

    public Slider slider;
	
	// Update is called once per frame
	void Update () {
        //Show the Slider Value in the text field
        this.GetComponent<Text>().text = "" + Mathf.RoundToInt((slider.value * 100)) + "%";
        
    }
}
