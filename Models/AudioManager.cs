using NAudio.Wave;
using System;

public class AudioManager
{
    private WaveOutEvent _audioOutput;
    private AudioFileReader _audioFile;

    public AudioManager(string audioFilePath)
    {
        _audioOutput = new WaveOutEvent();
        _audioFile = new AudioFileReader(audioFilePath);
        _audioOutput.Init(_audioFile);
    }

    public void PlayGameOverSound()
    {
        _audioOutput.Play();
    }

    public void StopPlayback()
    {
        _audioOutput.Stop();
    }

    public void Dispose()
    {
        _audioOutput.Dispose();
        _audioFile.Dispose();
    }
}

public class PlayerObject
{
    private AudioManager _audioManager;

    public PlayerObject(string audioFilePath)
    {
        _audioManager = new AudioManager(audioFilePath);
    }

    public void Die()
    {
        Console.WriteLine("Player died...");
        _audioManager.PlayGameOverSound();
    }
}
