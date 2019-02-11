using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TriggerBoxScript : MonoBehaviour {

    public MicInputManager2 mim;
    public NoteRecognizer nr;
    public float correctionTreshold;    //How much of a note has to be correct to count it as correct, in %

    GameObject currentHit;
    List<GameObject> threadMarkerList = new List<GameObject>();     //Contains the notes and chords stuck to the play stripe
    bool accuracy;                                                  
    bool hitThisNote = false;                                       

    int correctNotes = 0;
    int allNoteCounter = 0;

    private void Start()
    {
        if (PlayerPrefs.GetInt("HighAccuracy") == 0) accuracy = false;
        else accuracy = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        //Stop the game if the EndGame Objects hits the Trigger
        if (other.gameObject.name.Contains("EndGame"))
        {
            int score = (int)(((float)correctNotes / (float)allNoteCounter)*100);
            NoteSheetGen.start = false;
            PlayerPrefs.SetInt("Score", score);
            SceneManager.LoadScene(2);
            return;
        }

        GameObject temp = other.transform.parent.GetChild(0).gameObject;
     
        if (other.transform.parent.tag.Equals("chord"))
        {
            //Instantiate a copy of the chord, sticking to the play stripe
            Vector3 posObject = this.transform.GetChild(0).transform.position;
            GameObject threadMarker = Instantiate(temp.transform.parent.GetChild(1).gameObject, new Vector3(posObject.x, temp.transform.parent.position.y, posObject.z), Quaternion.identity);
            threadMarker.name = "Chord"+other.name;
            threadMarkerList.Add(threadMarker);
        }
        else
        {
            //Instantiate a copy of the thread marker sticking to the play stripe
            Vector3 posObject = this.transform.GetChild(0).transform.position;
            GameObject threadMarker = Instantiate(temp.transform.parent.GetChild(2).gameObject, new Vector3(posObject.x, temp.transform.parent.position.y, posObject.z), Quaternion.identity);
            threadMarker.name = other.name;
            threadMarkerList.Add(threadMarker);

            allNoteCounter++;
        }

    }

    private int currentAmountOfTriggers()
    {
        int count = 0;
        foreach( GameObject g in threadMarkerList)
        {
            if (!g.name.Contains("Chord")) count++;
        }
        return count;
    }

    private void OnTriggerExit(Collider other)
    {
        //Destroy the copy of the marker at the play stripe if the note is over
        foreach(GameObject g in threadMarkerList)
        {
            if (g.name.Contains(other.name))
            {
                Destroy(g);
                threadMarkerList.Remove(g);
                break;
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        Note currentNote;
        string midiNoteName;
        Note midiNote;
        if (!other.gameObject.name.Contains("EndGame") && !other.transform.parent.tag.Equals("chord"))
        {
            //Get the current note data
            try
            {
                currentNote = nr.currentNote;
                midiNoteName = other.transform.parent.name;
                midiNote = midiNoteNameToNote(midiNoteName);
            }
            catch (System.Exception)
            {
                currentNote = nr.currentNote;
                midiNoteName = "A2";
                midiNote = midiNoteNameToNote(midiNoteName);

            }


            //If it is the first hit of that note
            if (other.gameObject != currentHit && !other.gameObject.name.Contains("EndGame"))
            {
                hitThisNote = false;
                currentHit = other.gameObject;
                float noteLength = float.Parse(other.transform.parent.GetChild(1).name.Split('-')[1]) / 1000;
            }

            if (nr.compareNotes(currentNote, midiNote, accuracy))
            {
                //If the right note was played, change the color to green and add to the correct Notes counter
                other.transform.parent.GetChild(1).GetComponent<Renderer>().material.color = Color.green;
                if (!hitThisNote && currentAmountOfTriggers() <=1)
                {
                    hitThisNote = true;
                    correctNotes++;
                }
            }
            else
            {
                Debug.Log("MidiNote: " + midiNote.noteName + midiNote.octave + ", Played: " + currentNote.noteName + currentNote.octave + ", HalfstepsFromBase: " + nr.halfstepsBetweenCurrentAndBase);
            }
        }
    }

    //Returns a Note object for a notename from the midi file
    Note midiNoteNameToNote(string noteName)
    {
        Note tempNote = NoteRecognizer.baseNote;

        if (noteName.Length == 2)
        {
            char[] temp = noteName.ToCharArray();
            tempNote = new Note("" + temp[0], int.Parse("" + temp[1]));
        }
        else if (noteName.Length == 3)
        {
            char[] temp = noteName.ToCharArray();
            tempNote = new Note("" + temp[0] + temp[1], int.Parse("" + temp[2]));
        }

        return tempNote;
    }

    //Returns the Name of the Note
    public string intToNoteName(int note)
    {
        switch (note)
        {
            case 5:
                return "E4";
            case 4:
                return "B3";
            case 3:
                return "G3";
            case 2:
                return "D3";
            case 1:
                return "A2";
            case 0:
                return "E2";
            default:
                return "Error";
        }


    }



}
