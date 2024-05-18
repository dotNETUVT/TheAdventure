using NAudio.Wave;

public class AudioPlayer
{
    private WaveOutEvent _audioOutput;
    private AudioFileReader _musicFile;
    private float _soundLevel = 0.3f;
    private bool _playing;

    public AudioPlayer(string filePath)
    {
        _audioOutput = new WaveOutEvent();
        LoadFile(filePath);
    }

    private void LoadFile(string filePath)
    {
        _musicFile?.Dispose();
        _musicFile = new AudioFileReader(filePath) { Volume = _soundLevel };

        // used to play the music repeatedly, in a loop - some improvements about the break between loops could be made
        var repeatStream = new RepeatStream(_musicFile);
        _audioOutput.Init(repeatStream);
    }

    public void StartPlayback()
    {
        if (!_playing)
        {
            _audioOutput.Play();
            _playing = true;
        }
    }

    public void StopPlayback()
    {
        if (_playing)
        {
            _audioOutput.Stop();
            _playing = false;
        }
    }

    public void SwitchPlayback()
    {
        if (_playing)
        {
            StopPlayback();
        }
        else
        {
            StartPlayback();
        }
    }
}

public class RepeatStream : WaveStream
{
    private readonly WaveStream _source;

    public RepeatStream(WaveStream source)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
    }

    public override WaveFormat WaveFormat => _source.WaveFormat;

    public override long Length => _source.Length;

    public override long Position
    {
        get => _source.Position;
        set => _source.Position = value;
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        int bytesReadTotal = 0;

        while (bytesReadTotal < count)
        {
            int bytesRead = _source.Read(buffer, offset + bytesReadTotal, count - bytesReadTotal);
            if (bytesRead == 0)
            {
                _source.Position = 0;
            }
            bytesReadTotal += bytesRead;
        }
        return bytesReadTotal;
    }
}