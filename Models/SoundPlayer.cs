namespace TheAdventure.Models;

using NAudio.Wave;

public class SoundPlayer
{
    private IWavePlayer wavePlayer;
    private AudioFileReader audioFileReader;

    public SoundPlayer(string filePath)
    {
        wavePlayer = new WaveOutEvent();
        audioFileReader = new AudioFileReader(filePath);
        wavePlayer.Init(audioFileReader);
        wavePlayer.PlaybackStopped += OnPlaybackStopped;
    }

    private void OnPlaybackStopped(object sender, StoppedEventArgs e)
    {
        wavePlayer.Stop();
    }

    public void Play()
    {
        wavePlayer.Play();
    }

    public void Stop()
    {
        wavePlayer.Stop();
    }

    public void Dispose()
    {
        wavePlayer.Dispose();
        audioFileReader.Dispose();
    }
}