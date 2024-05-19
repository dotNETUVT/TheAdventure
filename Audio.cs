using NAudio.Wave;
using System;
namespace TheAdventure.Models;
public class Audio
{
    private WaveOutEvent _audioOutput;
    private AudioFileReader _audioFile;

    public Audio(string audioFilePath)
    {
        _audioOutput = new WaveOutEvent();
        _audioFile = new AudioFileReader(audioFilePath);
        _audioOutput.Init(_audioFile);
    }

    public void PlayGameOverSound()
    {
        _audioOutput.Play();
    }
}