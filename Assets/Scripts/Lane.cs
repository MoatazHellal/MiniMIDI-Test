using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Multimedia;
using Melanchall.DryWetMidi.Core;

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lane : MonoBehaviour
{
    Animation anim;
    private InputDevice inputDevice;
    private Queue<Melanchall.DryWetMidi.MusicTheory.NoteName> midiNoteQueue = new Queue<Melanchall.DryWetMidi.MusicTheory.NoteName>();

    public Melanchall.DryWetMidi.MusicTheory.NoteName noteName;
    public KeyCode input;
    public GameObject notePrefab;
    List<Note> notes = new List<Note>();
    public List<double> timeStamps = new List<double>();

    int spawnIndex = 0;
    int inputIndex = 0;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animation>();

        foreach (var device in InputDevice.GetAll())
        {
            inputDevice = device;
            break; // take only the first one 
        }

        if (inputDevice == null)
        {
            return;
        }

        inputDevice.EventReceived += OnMidiInput;
        inputDevice.StartEventsListening();
    }
    public void SetTimeStamps(Melanchall.DryWetMidi.Interaction.Note[] array)
    {
        foreach (var note in array)
        {
            if (note.NoteName == noteName)
            {
                var metricTimeSpan = TimeConverter.ConvertTo<MetricTimeSpan>(note.Time, SongManager.midiFile.GetTempoMap());
                timeStamps.Add((double)metricTimeSpan.Minutes * 60f + metricTimeSpan.Seconds + (double)metricTimeSpan.Milliseconds / 1000f);
            }
        }
    }

    private void OnMidiInput(object sender, MidiEventReceivedEventArgs e)
    {
        if (e.Event is NoteOnEvent noteOn && noteOn.Velocity > 0)
        {
            midiNoteQueue.Enqueue((Melanchall.DryWetMidi.MusicTheory.NoteName)(noteOn.NoteNumber % 12)); // MIDI input queue 
        }
    }


    // Update is called once per frame
    void Update()
    {
        if (spawnIndex < timeStamps.Count)
        {
            if (SongManager.GetAudioSourceTime() >= timeStamps[spawnIndex] - SongManager.Instance.noteTime)
            {
                var note = Instantiate(notePrefab, transform);
                notes.Add(note.GetComponent<Note>());
                note.GetComponent<Note>().assignedTime = (float)timeStamps[spawnIndex];
                spawnIndex++;
            }
        }

        if (inputIndex < timeStamps.Count)
        {
            double timeStamp = timeStamps[inputIndex];
            double marginOfError = SongManager.Instance.marginOfError;
            double audioTime = SongManager.GetAudioSourceTime() - (SongManager.Instance.inputDelayInMilliseconds / 1000.0);

            bool keyWasPressed = Input.GetKeyDown(input);

            while (midiNoteQueue.Count > 0)
            {
                Melanchall.DryWetMidi.MusicTheory.NoteName midiNote = midiNoteQueue.Dequeue();

                if (midiNote == noteName)
                {
                    keyWasPressed = true;
                    break;
                }
            }

            if (keyWasPressed)
            {
                if (Math.Abs(audioTime - timeStamp) < marginOfError)
                {
                    Hit();
                    Destroy(notes[inputIndex].gameObject);
                    inputIndex++;
                }
            }
            if (timeStamp + marginOfError <= audioTime)
            {
                Miss();
                inputIndex++;
            }
        }

    }
    private void Hit()
    {
        anim.Play("Hit");
        ScoreManager.Hit();
    }
    private void Miss()
    {
        anim.Play("Miss");
        ScoreManager.Miss();
    }
}
