using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


public class MicInputManager2 : MonoBehaviour {

    public float recognizeVolumeThreshold;
    public NoteRecognizer nr;
    string device;                  //Contains the mic name
    AudioSource aud;                //Mic Output  

    // Use this for initialization
    void Start () {
        micInit();
        //StartCoroutine(micReset());
        if (PlayerPrefs.GetInt("muteInput") == 1)
        {
            Debug.Log("Hey");
            aud.outputAudioMixerGroup.audioMixer.SetFloat("MicInput", -25.0f);
        }
        else
        {
            aud.outputAudioMixerGroup.audioMixer.SetFloat("MicInput", 0.0f);
        }
    }
	
	// Update is called once per frame
	void Update () {
        nr.specturmToNote(getSpectrum(), AudioSettings.outputSampleRate);
	}

    //Initialize the microphone and maps it to an audio source
    void micInit()
    {
        if (device == null) device = Microphone.devices[0];
        Debug.Log(device);
        aud = GetComponent<AudioSource>();
        aud.clip = Microphone.Start(device, true, 1800, 44100);
        aud.loop = true;
        while (!(Microphone.GetPosition(null) > 0)) { }
        aud.Play();
    }

    //The mic starts to get a loud noise signal after a while, re-initializing fixes that
    IEnumerator micReset()
    {
        while (true)
        {
            yield return new WaitForSeconds(15.0f);
            micInit();
        }

    }

    //Turns the audio from the mic into an array, each field corresponds to a range of 5.38Hz and the value corresponds to the amplitude
    public float[] getSpectrum()
    {
        float[] spectrum = new float[8192];     //8192 fields give you a max error of 5.38Hz 
        AudioListener.GetSpectrumData(spectrum, 0, FFTWindow.BlackmanHarris);
        return spectrum;
    }

}
