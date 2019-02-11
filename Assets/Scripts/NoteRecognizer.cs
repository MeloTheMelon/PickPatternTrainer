using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteRecognizer : MonoBehaviour {

    /*
    Frequencies of open guitar strings:
    E2 = 82.41Hz
    A2 = 110Hz
    D3 = 146.83Hz
    G3 = 196Hz
    B3 = 246.94Hz
    E4 = 329.63Hz
    */


    //As base note I took E2 because it is the lowest note on a guitar, can be changed to whatever other note if needed
    public string baseNoteName = "E";
    public int baseNoteOctave = 2;
    public float baseNoteFrequency = 82.41f;

    public static Note baseNote = new Note("E", 2);

    //Volume-threshold so it won't try to recognize Notes if nothing is played
    public float recognizeNoteVolumeThreshold;

    public Note currentNote;                                //current recognized Note
    public int halfstepsBetweenCurrentAndBase;
    Vector2 usefullFrequencyPart = new Vector2(25, 450);    //Frequencies of guitar notes won't be lower than 73Hz or higher than 1390Hz (highest possible on standard guitar with 22 threads)
                                                            //Thus we can ignore all frequencies higher or lower and it is a lot faster to work with 95 values than 8192

    float a = Mathf.Pow(2.0f, (1.0f / 12.0f));              //Factor needed for the calculation of the halfsteps between the baseNote and the playedNote
    float logA;                                             //ln(a)
    

	// Use this for initialization
	void Start () {
		logA = Mathf.Log(a);                                //Precalculate the ln(a), it won't change and is needed every frame
    }

    //Filters out noise and increases the harmonic signal
    float[] harmonicProductSpectrum(float[] spectrum)
    {
        float[] copy = spectrum;
        for (int compression = 2; compression < 4; compression++)
        {
            for (int i = 0; i < spectrum.Length; i++)
            {
                float factor = 0;
                if (1 + i * compression < copy.Length)
                    factor = copy[1 + i * compression];
                spectrum[i] = spectrum[i] * factor;
            }
        }
        return spectrum;

    }

    //Analyze the Frequency-Spectrum and calculate the current note
    public void specturmToNote(float[] spectrum, float sampleRate)
    {
        //Get the index of the first noteabale local maximum => Frequency of the played note
        int max = getLocalMaxIndex(spectrum);
        Debug.Log("Max: " + max);
        //Don't check if nothing is played
        if (max > recognizeNoteVolumeThreshold)
        {
            spectrum = harmonicProductSpectrum(spectrum);
            float freq = max * (sampleRate/2) / (spectrum.Length);  //Calculate the frequency of the index from the local maximum
            if (freq > 75.0f)                                       //Lower values are just noise, so they can be ignored, the lowest possible played Frequence would be 82Hz
            {
                int halfsteps = frequencyToNote(freq);
                halfstepsBetweenCurrentAndBase = halfsteps;
                currentNote = new Note(halfsteps, baseNote);
            }
            else
            {
                currentNote = new Note("noNote", 0);
            }
        }
        else
        {
            currentNote = new Note("noNote", 0);
        }
    }

    //Find the highest value in the spectrum, was used to get the frequency, but is extremly sensitive to noise
    //Without noise it would be correct all the time, not like the local maximum variant
    int getMaxIndex(float[] spectrum)
    {
        int maxIndex = -1;
        float maxValue = 0;

        for(int i = (int) usefullFrequencyPart.x; i < usefullFrequencyPart.y; i++)
        {

            if (spectrum[i] > maxValue)
            {
                maxValue = spectrum[i];
                maxIndex = i;
            }
        }
        return maxIndex;
    }

    //Calculates the volume of the input
    float getVolume(float[] spectrum)
    {
        float vol = 0;
        foreach(float f in spectrum)
        {
            vol = Mathf.Max(vol, f);
        }
        return vol;
    }

    //Find the Index of the first local maximum, which is used to calculate the frequency of the note
    int getLocalMaxIndex(float[] spectrum)
    {

        float volume = getVolume(spectrum);

        for (int i = (int) usefullFrequencyPart.x; i < usefullFrequencyPart.y; i++)
        {
            if (spectrum[i]>volume*0.10f && spectrum[i] > spectrum[i - 1] && spectrum[i] > spectrum[i + 1])
            {
                return i;
            }
        }
        return 0;
    }

    //Turns a frequency into an int, which tells how many halfnotes the played note is away from the baseNote
    int frequencyToNote(float playedNoteFrequency)
    {
        float halfstepsBetween = Mathf.Log(playedNoteFrequency / baseNoteFrequency) / logA;
        return Mathf.RoundToInt(halfstepsBetween);
    }

    //Compare if two notes are the "same", setting the lessAccurate to true will return true if it is the exact note or one above or below
    //Solves errors with noise and slightly mistuned strings 
    public bool compareNotes(Note a, Note b, bool lessAccurate)
    {
        if (lessAccurate)
        {
            if ((Mathf.Abs(a.noteToHalfsteps() - b.noteToHalfsteps())) < 2)
            {
                return true;
            }
        }
        else
        {
            if ((Mathf.Abs(a.noteToHalfsteps() - b.noteToHalfsteps())) == 0)
            {
                return true;
            }
        }

        
        return false;
    }

}

//A Object to work with notes, it contains the octave and the note name
public class Note
{
    public string noteName;
    public int octave;

    //Constructor to create a note just from the amount of halfnotes it is away from the baseNote
    //Just saves a lot of calculations and extra functions
    public Note(int halfsteps, Note baseNote)
    {
        noteName = intToNoteName((noteNameToInt(baseNote.noteName) + halfsteps) % 12);
        octave = baseNote.octave + ((halfsteps + noteNameToInt(baseNote.noteName)) / 12);
    }
    
    //Constructor to create a note the "normal" way
    public Note(string noteName, int octave)
    {
        this.noteName = noteName;
        this.octave = octave;
    }

    //Takes an int between 0 and 11 and maps it to the a note
    public string intToNoteName(int note)
    {
        switch (note)
        {
            case 0:
                return "C";
            case 1:
                return "C#";
            case 2:
                return "D";
            case 3:
                return "D#";
            case 4:
                return "E";
            case 5:
                return "F";
            case 6:
                return "F#";
            case 7:
                return "G";
            case 8:
                return "G#";
            case 9:
                return "A";
            case 10:
                return "A#";
            case 11:
                return "B";
            default:
                Debug.Log("Note must be between 0 and 11");
                return "0";
        }
    }

    //Takes a note name and maps it to the corresponding int value
    public int noteNameToInt(string note)
    {
        switch (note)
        {
            case "C":
                return 0;
            case "C#":
                return 1;
            case "D":
                return 2;
            case "D#":
                return 3;
            case "E":
                return 4;
            case "F":
                return 5;
            case "F#":
                return 6;
            case "G":
                return 7;
            case "G#":
                return 8;
            case "A":
                return 9;
            case "A#":
                return 10;
            case "B":
                return 11;
            case "noNote":
                Debug.Log("No note is played");
                return 13;
            default:
                Debug.Log("Unkown note name: "+note);
                return 13;
        }
    }

    //Turns a note into an int, which corresponds to the amount of halfnotes between the baseNote and the given Note
    public int noteToHalfsteps()
    {
        return (this.octave - NoteRecognizer.baseNote.octave) *12 + (noteNameToInt(this.noteName) - noteNameToInt(NoteRecognizer.baseNote.noteName));
    }

}
