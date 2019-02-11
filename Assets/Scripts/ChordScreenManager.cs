using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChordScreenManager : MonoBehaviour {

    public GameObject chordScreenCanvas;            //Contains the whole UI Canvas
    public GameObject chordScreenImage;             //Contains the image showing the Chord
    public CountdownScript cds;                     //Starts the note sheet movement
    public List<Sprite> chordTutorialSprites;       //List of all the Chord Images   
    int chordCounter = 0;                           //Counts which chord in the list is shown
    int usedChordsAmount = 10;                      //Amount of Chords in that song
    GameObject nextButton;                          //Button to get to the next Chord / Start the game
    List<string> chordData = new List<string>();    //All used Chords from that song

	// Use this for initialization
	void Start () {
        nextButton = chordScreenCanvas.transform.Find("NextButton").gameObject;        //Find the Next Button
        
    }
	
	// Update is called once per frame
	void Update () {
        //Change the text in the Next Button to start if the last chord is shown
        if (chordCounter == usedChordsAmount-1)
        {
            nextButton.GetComponentInChildren<Text>().text = "Start";
        }
	}

    //Enables the chord screen
    public void openChordScreen(List<string> usedChords)
    {
        usedChordsAmount = usedChords.Count;            //Set the usedChordAmount
        chordScreenCanvas.SetActive(true);              //Enable the Screen
        chordData = usedChords;                         //Save the Chord List
        chordScreenImage.GetComponent<Image>().sprite = chordTutorialSprites[noteDataToChordIndex(usedChords[0])];      //Set the first Image
        Debug.Log("Chord Amount: " + usedChordsAmount);
    }

    //Returns the Index of the chord in the list of images
    public static int noteDataToChordIndex(string noteName)
    {
        switch (noteName)
        {
            case "A1":          //A      
                return 0;
            case "A2":          //AM
                return 1;
            case "A3":          //A7
                return 2;
            case "A4":          //ASus2
                return 3;
            case "B3":          //B7
                return 4;
            case "C1":          //C
                return 5;
            case "C3":          //C7
                return 6;
            case "D1":          //D
                return 7;
            case "D2":          //DM
                return 8;
            case "D3":          //D7
                return 9;
            case "E1":          //E
                return 10;
            case "E2":          //EM
                return 11;
            case "E3":          //E7
                return 12;
            case "F1":          //F
                return 13;
            case "G1":          //G
                return 14;
            case "G3":          //G7
                return 15;
            default:
                break;
        }
        return 0;
    }

    //Returns the name of the chord given the NoteName in the midi-file
    public static string noteDataToChordName(string noteName)
    {
        switch (noteName)
        {
            case "A1":          //A      
                return "A";
            case "A2":          //AM
                return "Am";
            case "A3":          //A7
                return "A7";
            case "A4":          //ASus2
                return "Asus2";
            case "B3":          //B7
                return "B7";
            case "C1":          //C
                return "C";
            case "C3":          //C7
                return "C7";
            case "D1":          //D
                return "D";
            case "D2":          //DM
                return "Dm";
            case "D3":          //D7
                return "D7";
            case "E1":          //E
                return "E";
            case "E2":          //EM
                return "Em";
            case "E3":          //E7
                return "E7";
            case "F1":          //F
                return "F";
            case "G1":          //G
                return "G";
            case "G3":          //G7
                return "G7";
            default:
                break;
        }
        return noteName;
    }

    public void onNextButton()
    {
        chordCounter++;
        
        //Changes the Chord Image on click and start the game / disables the Chord Canvas
        if (chordCounter < chordData.Count)
        {
            chordScreenImage.GetComponent<Image>().sprite = chordTutorialSprites[noteDataToChordIndex(chordData[chordCounter])];
        }
        else
        {
            StartCoroutine(cds.countdown(3));
            chordScreenCanvas.SetActive(false);
        }
    }


}
