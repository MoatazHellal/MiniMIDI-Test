using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using System.IO;
using System;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SongManager : MonoBehaviour
{
    public static SongManager Instance;
    public AudioSource audioSource;

    public Lane[] lanes;

    public float songDelayInSeconds;
    public double marginOfError;

    public int inputDelayInMilliseconds;
    public Button replayButton;

    public string fileName;
    public float noteTime;
    public float noteSpawnY;
    public float noteTapY;
    public float noteDespawnY
    {
        get
        {
            return noteTapY - (noteSpawnY - noteTapY);
        }
    }

    public static MidiFile midiFile;



    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
        ReadMidiFile();

        replayButton.gameObject.SetActive(false);

    }

    private void ReadMidiFile()
    {
        midiFile = MidiFile.Read(Application.streamingAssetsPath + "/" + fileName);
        GetDataFromMidi();
    }
    public void GetDataFromMidi()
    {
        var notes = midiFile.GetNotes();
        var array = new Melanchall.DryWetMidi.Interaction.Note[notes.Count];
        notes.CopyTo(array, 0);

        foreach (var lane in lanes) lane.SetTimeStamps(array);

        Invoke(nameof(StartSong), songDelayInSeconds);
    }
    public void StartSong()
    {
        audioSource.Play();
    }
    public static double GetAudioSourceTime()
    {
        return (double)Instance.audioSource.timeSamples / Instance.audioSource.clip.frequency;
    }

    public void ReplaySong()
    {
        // Reload scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void Update()
    {
        if (!audioSource.isPlaying && !replayButton.gameObject.activeSelf)
        {
            // The song has finished
            replayButton.gameObject.SetActive(true);
        }
    }
}
