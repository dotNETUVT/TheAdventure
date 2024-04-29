namespace TheAdventure;

using CSCore;
using CSCore.SoundOut;
using CSCore.Codecs;

public class Musicz
{
    private readonly ISoundOut _audioOutput;
    private readonly IWaveSource _waveSource;
    private bool _isPlaying; // boolean value to determine if music is playing

    public Musicz(string filePath)
    {
        _waveSource = CodecFactory.Instance.GetCodec(filePath)
            .ToSampleSource()
            .ToWaveSource();
        _audioOutput = new WasapiOut();
        _audioOutput.Initialize(_waveSource);
        _isPlaying = false;
    }

    public void Start()
    {
        _audioOutput.Play();
        _isPlaying = true;
    }

    public void Halt()
    {
        _audioOutput.Pause();
        _isPlaying = false;
    }
    
    public void TogglePlayPause() // toggle function 
    {
        if (_isPlaying)
        {
            Halt();
        }
        else
        {
            Start();
        }
    }

    public void Release()
    {
        
        _audioOutput.Dispose();
        _waveSource.Dispose();
    }
}
