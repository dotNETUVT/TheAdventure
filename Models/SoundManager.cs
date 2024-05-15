namespace TheAdventure.Models;
using NAudio.Wave;

public class SoundManager
{
    private readonly WaveOutEvent outputDevice;
    private readonly AudioFileReader audioFile;

    public SoundManager(string filePath)
    {
        outputDevice = new WaveOutEvent();
        audioFile = new AudioFileReader(filePath);
        outputDevice.Init(audioFile);
    }

    public void Play()
    {
        outputDevice.Play();
    }

    public void Stop()
    {
        outputDevice.Stop();
    }
}