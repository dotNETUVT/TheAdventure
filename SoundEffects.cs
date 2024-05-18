namespace TheAdventure;
using NAudio.Wave;

public class SoundManager
{
    private WaveOutEvent _waveOut;

    public SoundManager()
    {
        _waveOut = new WaveOutEvent();
    }

    public void PlaySound(string soundFilePath)
    {
        try
        {
            // In order to avoid hearing the sound each frame, you only hear it once
            // And it will continue only if the action is still being performed
            if (_waveOut.PlaybackState == PlaybackState.Playing)
            {
                Console.WriteLine("Cannot play sound: Already playing.");
                return;
            }
            
            var audioFile = new AudioFileReader(soundFilePath);
            _waveOut.Init(audioFile);
            _waveOut.Play();
            
            //We dispose the sound after playing it in full.
            //To free up resources.
            _waveOut.PlaybackStopped += (sender, args) =>
            {
                _waveOut.Dispose();
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error playing sound: {ex.Message}");
        }
    }
}
