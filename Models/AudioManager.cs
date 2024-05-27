using NAudio.Wave;
using System;

public class AudioManager : IDisposable
{
    private WaveOutEvent _audioOutput;
    private AudioFileReader _audioFile;
    private AudioFileReader _lifeLostFile;

    public AudioManager(string gameOverFilePath, string lifeLostFilePath)
    {
        _audioOutput = new WaveOutEvent();
        _audioFile = new AudioFileReader(gameOverFilePath);
        _lifeLostFile = new AudioFileReader(lifeLostFilePath);
        _audioOutput.Init(_audioFile);
    }

    public void PlayGameOverSound()
    {
        _audioOutput.Init(_audioFile); // Re-initialize to reset position
        _audioOutput.Play();
    }

    public void PlayLifeLostSound()
    {
        _audioOutput.Init(_lifeLostFile); // Re-initialize to reset position
        _audioOutput.Play();
    }

    public void Dispose()
    {
        _audioOutput.Dispose();
        _audioFile.Dispose();
        _lifeLostFile.Dispose();
    }
}
