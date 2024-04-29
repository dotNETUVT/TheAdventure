using CSCore;
using CSCore.SoundOut;
using CSCore.Codecs;

public class MusicPlayer
{
    private readonly ISoundOut _soundOut;
    private readonly IWaveSource _waveSource;

    public MusicPlayer(string filePath)
    {
        
        _waveSource = CodecFactory.Instance.GetCodec(filePath)
            .ToSampleSource()
            .ToWaveSource();
        _soundOut = new WasapiOut();
        _soundOut.Initialize(_waveSource);
    }

    public void Play()
    {
        _soundOut.Play();
    }

    public void Stop()
    {
        _soundOut.Stop();
    }

    public void Dispose()
    {
        _soundOut.Dispose();
        _waveSource.Dispose();
    }
}
