using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NAudio;
using NAudio.Midi;

public class NoteSheetGen : MonoBehaviour {

    public string midiFilePath;

    float conversionUnit = 1000.0f; //100ms = 1 Unity Unit
    public GameObject StartBlockPrefab, LinePrefab, ThreadMarkerPanel, EndGameObject;
    public CountdownScript cds;
    MidiFile midi;
    NoteRecognizer nr = new NoteRecognizer();

    List<string> usedChords = new List<string>();

    GameObject Map;

    public static bool start = false;

    int capoAtThread;

    public GameObject chordMarker;

    // Use this for initialization
    void Start () {
        
        capoAtThread = PlayerPrefs.GetInt("capoAtThread");                  //Set the shift from using a capo
      
        if (PlayerPrefs.GetString("selectedMidiFile").Contains(".mid"))     //Load the chosen Song
        {
            midi = new MidiFile(PlayerPrefs.GetString("selectedMidiFile"));
        }

        float speedFactor = PlayerPrefs.GetFloat("speedFactor");            //Set the Speed
        conversionUnit = conversionUnit * speedFactor;                      //Adjust conversion Unit according to the Speedfactor
       

        Map = new GameObject();                                             //ParentObject of all the spawned Indicators
        Map.name = "Map";

        float lastNoteEndingTime = 0;                                       //Contains the time of the last note in the song
        float songLength = 0;                                               //Contains the Song length
        
        for (int i = 0; i<PlayerPrefs.GetInt("Repetitions"); i++) {         //Repeat for the amount in the Repetitions field
            foreach (MidiEvent note in midi.Events[1])                      //Loop over all notes
            {
                if (MidiEvent.IsNoteOn(note))                               //Filter out when notes start
                {
                    NoteOnEvent tempEvent = (NoteOnEvent)note;
                    lastNoteEndingTime = note.AbsoluteTime + tempEvent.NoteLength;                      //Set it at every note, so we have the time of the End of the song after the loop

                    char[] tempAr = tempEvent.NoteName.ToCharArray();                                   //Split the NoteName so the Thread and String can be calculated
                    if (int.Parse("" + tempAr[tempAr.Length - 1]) < NoteRecognizer.baseNote.octave)     //Check if the note is playable
                    {
                        Debug.LogError("Note not playable");
                        this.GetComponent<ErrorScreen>().openError();
                        break;
                    }
                    //Set the EndGameObject at the end of the song
                    EndGameObject.transform.position = new Vector3(((note.AbsoluteTime + tempEvent.NoteLength + i * songLength) / conversionUnit)+1, 0, -0.5f);
                    try
                    {
                        //Spawn a Marker showing the Thread and String on the map
                        spawnThreadMarkers(note.AbsoluteTime + i * songLength, tempEvent.NoteLength, tempEvent.NoteName);
                    }
                    catch (System.Exception)
                    {
                        this.GetComponent<ErrorScreen>().openError();
                    }

                }
                if (i == 0) songLength = lastNoteEndingTime;
            }
            try
            {
                //Second Track contains the Chord Data
                //Do the same as above, just spawn ChordMarkers instead of ThreadMarkers
                foreach (MidiEvent chord in midi.Events[2])
                {
                    if (MidiEvent.IsNoteOn(chord))
                    {
                        NoteOnEvent tempEvent2 = (NoteOnEvent)chord;
                        usedChords.Add(tempEvent2.NoteName);            //Add the Chords to a list for the Chord Tutorial Screen

                        spawnChordMarkers(chord.AbsoluteTime + i * songLength, tempEvent2.NoteLength, tempEvent2.NoteName);
                        
                    }
                }
            }
            catch (System.Exception)
            {
                //Ignore Chords if there is no data
                Debug.Log("No Chord Data");
            }
            
        }

        if(usedChords.Count != 0)
        {
            //Opens the Chord Tutorial Screen
            this.GetComponent<ChordScreenManager>().openChordScreen(usedChords);
        }
        else
        {
            //Start the countdown
            StartCoroutine(cds.countdown(3));
        }

        GameObject lines = new GameObject();
        //Spawn the background
        for(float i = -0.5f; i <= 10; i++)
        {
            lines = Instantiate(LinePrefab, new Vector3(i, 0, 0), Quaternion.identity);
        }


    }

    private void Update()
    {
        //Move the map forward
        if (start)
        {
            Map.transform.position += (new Vector3(-1, 0, 0) * Time.deltaTime);
            EndGameObject.transform.position += (new Vector3(-1, 0, 0) * Time.deltaTime);
        }
    }
    
    //Instantiates the Thread Markers at the right Thread and String
    void spawnThreadMarkers(float absTime, float length, string NoteName)
    {

        Vector3 pos = Vector3.zero;
        pos.x = (absTime / conversionUnit);                         //Position of the Marker on the Map
        Vector2 location = NoteNameToStringAndThread(NoteName);     //Calculate Thread and String from the NoteName
        pos.y = -0.625f + location.x * 0.25f;                       //Move it accordingly to that data
        pos.z = -0.52f;                                             //Z Value so it is infront of everything

        GameObject temp = Instantiate(ThreadMarkerPanel, pos, Quaternion.identity); //Instantiate the Marker
        temp.GetComponentInChildren<Text>().text = ""+(int)location.y;              //Write the Thread Data
        temp.name = NoteName;                                                       
        GameObject statusCube = temp.transform.GetChild(1).transform.gameObject;    //Contains the Collider and is the red box showing the note
        statusCube.transform.localScale = new Vector3(length / (conversionUnit), statusCube.transform.localScale.y, statusCube.transform.localScale.z);
        statusCube.transform.position = new Vector3((length / (2 * conversionUnit)) + temp.transform.position.x, statusCube.transform.position.y, statusCube.transform.position.z);
        statusCube.GetComponent<Renderer>().material.color = Color.red;
        statusCube.name = absTime + "-" + length;                                   //Data for the TriggerBox is stored in the name of the object
        temp.transform.parent = Map.transform;                                      //Set Map as parent object

    }

    //Instantiate a Chord Marker
    void spawnChordMarkers(float absTime, float length, string NoteName)
    {

        Vector3 pos = Vector3.zero;
        pos.x = (absTime / conversionUnit);
        pos.y = -0.77f;                         //Under all the Notes
        pos.z = -0.52f;

        GameObject temp = Instantiate(chordMarker, pos, Quaternion.identity);           //Spawns the Marker
        temp.GetComponentInChildren<Text>().text = ""+ChordScreenManager.noteDataToChordName(NoteName); //Set the Chord Name

        temp.name = ChordScreenManager.noteDataToChordName(NoteName);
        temp.tag = "chord";                                                             //Tag it as a Chord
        BoxCollider bc = temp.GetComponentInChildren<BoxCollider>();
        bc.size = new Vector3(length / (conversionUnit), bc.size.y, bc.size.z);
        bc.center = new Vector3(bc.size.x / 2, bc.center.y, bc.center.z);
        Vector3 canvasPos = temp.transform.GetChild(1).transform.position;
        GameObject statusCube = temp.transform.GetChild(0).transform.gameObject;
        statusCube.transform.localScale = new Vector3(1, statusCube.transform.localScale.y, statusCube.transform.localScale.z);
        statusCube.transform.position = new Vector3( temp.transform.position.x, statusCube.transform.position.y, statusCube.transform.position.z);
        statusCube.GetComponent<Renderer>().material.color = Color.cyan;
        statusCube.name = absTime + "-" + length;
        temp.transform.GetChild(1).transform.position = new Vector3(statusCube.transform.position.x, canvasPos.y, canvasPos.z);
        temp.transform.parent = Map.transform;
    }

    //Convert a note name to the fitting string and thread
    Vector2 NoteNameToStringAndThread(string NoteName)
    {
        Vector2 result = Vector2.zero;
        Note tempNote = new Note("noNote", 13);
        
        if(NoteName.Length == 2)
        {
            char[] temp = NoteName.ToCharArray();
            tempNote = new Note(""+temp[0], int.Parse("" + temp[1]));
        }else if(NoteName.Length == 3)
        {
            char[] temp = NoteName.ToCharArray();
            tempNote = new Note("" + temp[0]+temp[1], int.Parse("" + temp[2]));
        }
        int halfsteps = tempNote.noteToHalfsteps();

        result = halfstepToString(halfsteps);
        
        return result;
    }

    //Calculates the Thread and String for a given amount of steps from the base note
    Vector2 halfstepToString(int halfsteps)
    {
        Vector2 result = Vector2.zero;
        halfsteps -= capoAtThread;
        if (halfsteps < 5)
        {
            result.x = 0;
            result.y = halfsteps;
        }else if(halfsteps<10 && halfsteps >= 5)
        {
            result.x = 1;
            result.y = halfsteps - 5;
        }
        else if (halfsteps < 15 && halfsteps >= 10)
        {
            result.x = 2;
            result.y = halfsteps - 10;
        }
        else if (halfsteps < 19 && halfsteps >= 15)
        {
            result.x = 3;
            result.y = halfsteps - 15;
        }
        else if (halfsteps < 24 && halfsteps >= 19)
        {
            result.x = 4;
            result.y = halfsteps - 19;
        }
        else if (halfsteps >= 24)
        {
            result.x = 5;
            result.y = halfsteps - 24;
        }
        else
        {
            Debug.Log("Negativ amount of halfsteps");
            result.x = 13;
        }


        return result;
    }

}
