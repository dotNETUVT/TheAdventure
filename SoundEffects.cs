using CSCore;
using CSCore.SoundOut;
using CSCore.Codecs;

public class SoundEffects
{
    private readonly ISoundOut _sound;
    private readonly IWaveSource _source;

    public SoundEffects(string filePath)
    {

        _source = CodecFactory.Instance.GetCodec(filePath)
            .ToSampleSource()
            .ToWaveSource();
        _sound = new WasapiOut();
        _sound.Initialize(_source);
    }

    public void Play()
    {
        _sound.Play();
    }
}