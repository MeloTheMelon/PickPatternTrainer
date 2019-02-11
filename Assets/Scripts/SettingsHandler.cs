using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SettingsHandler : MonoBehaviour {

    public static string selectedMidiFile = "";
    public static float speedMultiplier;
    
    public Dropdown midiFileChooser;
    public Slider speedPercentage;
    public Toggle accuracy;
    public InputField capoAtThreadField;
    public InputField repetitionsField;
    public Toggle mute;

    public bool editorMode = false;     //Set True to find the Midi files when run in editor

    string path;
    List<string> foundMidiFiles = new List<string>();

	// Use this for initialization
	void Start () {
        path = Application.dataPath;
        if(editorMode) path = path.Substring(0, path.Length - 7);
        else path = path.Substring(0, path.Length - 24);

        int width = 1920;
        int height = 1080;
        bool isFullScreen = false;
        int desiredFPS = 60;

        Screen.SetResolution(width, height, isFullScreen, desiredFPS);

        foreach (string file in System.IO.Directory.GetFiles(path))
        {
            string[] temp = file.Split('\\');
            string fileName = temp[temp.Length - 1];
            if (fileName.Split('.')[1].Equals("mid") && fileName.Split('.').Length <=2)
            {
                foundMidiFiles.Add(fileName);
            }

        }
        
        midiFileChooser.AddOptions(foundMidiFiles);

	}
	
	// Update is called once per frame
	void Update () {
		
	}


    //Save the SettingsData in the PlayerPrefs
    public void settingsMenuSubmitButton()
    {
        
        selectedMidiFile = foundMidiFiles[midiFileChooser.value];
        speedMultiplier = speedPercentage.value;
        PlayerPrefs.SetString("selectedMidiFile", foundMidiFiles[midiFileChooser.value]);
        PlayerPrefs.SetFloat("speedFactor", speedMultiplier);
        if (accuracy.isOn)
        {
            PlayerPrefs.SetInt("HighAccuracy", 1);
        }
        else
        {
            PlayerPrefs.SetInt("HighAccuracy", 0);
        }
        if(capoAtThreadField.text != "")
        {
            PlayerPrefs.SetInt("capoAtThread", int.Parse(capoAtThreadField.text));
        }
        else
        {
            PlayerPrefs.SetInt("capoAtThread", 0);
        }
        if(repetitionsField.text != "")
        {
            PlayerPrefs.SetInt("Repetitions", int.Parse(repetitionsField.text));
        }
        else
        {
            PlayerPrefs.SetInt("Repetitions", 1);
        }
        if (mute.isOn)
        {
            PlayerPrefs.SetInt("muteInput", 1);
        }
        else
        {
            PlayerPrefs.SetInt("muteInput", 0);
        }



        //Load GameScene
        SceneManager.LoadScene(1);
    }

    public void quitButton()
    {
        Application.Quit();
    }

}
