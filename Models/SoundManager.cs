using System;
using NAudio.Wave;

public class SoundManager
{
    private WaveOutEvent _outputDevice;
    private AudioFileReader _sprintSound;
    private readonly string _typeSprint = "SprintStart";

    public SoundManager()
    {
        _sprintSound = new AudioFileReader("Assets\\turbo.mp3");
        _outputDevice = new WaveOutEvent();
        _outputDevice.Init(_sprintSound);
    }

    public void Play(string soundEvent)
    {
        try
        {
            if (soundEvent == _typeSprint)
            {
                _sprintSound.Position = 0;
                _outputDevice.Play();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"nu merge sunetu");
        }
    }
}
