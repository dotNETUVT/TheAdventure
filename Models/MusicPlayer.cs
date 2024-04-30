using NAudio.Wave;

public class MusicPlayer
{
    private WaveOutEvent _outputDevice;
    private AudioFileReader _audioFile;
    private float _volume = 0.5f;
    private bool _isPlaying;

    public MusicPlayer(string filePath)
    {
        _outputDevice = new WaveOutEvent();
        LoadMusic(filePath);
    }

    private void LoadMusic(string filePath)
    {
        _audioFile?.Dispose();
        _audioFile = new AudioFileReader(filePath) { Volume = _volume };

        // used to play the music repeatedly, in a loop - some improvements about the break between loops could be made
        var loopStream = new LoopStream(_audioFile);
        _outputDevice.Init(loopStream);
    }

    public void PlayMusic()
    {
        if (!_isPlaying)
        {
            _outputDevice.Play();
            _isPlaying = true;
        }
    }

    public void StopMusic()
    {
        if (_isPlaying)
        {
            _outputDevice.Stop();
            _isPlaying = false;
        }
    }

    public void ToggleMusic()
    {
        if (_isPlaying)
        {
            StopMusic();
        }
        else
        {
            PlayMusic();
        }
    }

    public float Volume
    {
        get => _volume;
        set
        {
            _volume = Math.Clamp(value, 0.0f, 1.0f);
            _audioFile.Volume = _volume;
        }
    }
}

public class LoopStream : WaveStream
{
    private readonly WaveStream _sourceStream;

    public LoopStream(WaveStream sourceStream)
    {
        _sourceStream = sourceStream ?? throw new ArgumentNullException(nameof(sourceStream));
    }

    public override WaveFormat WaveFormat => _sourceStream.WaveFormat;

    public override long Length => _sourceStream.Length;

    public override long Position
    {
        get => _sourceStream.Position;
        set => _sourceStream.Position = value;
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        int totalBytesRead = 0;

        while (totalBytesRead < count)
        {
            int bytesRead = _sourceStream.Read(buffer, offset + totalBytesRead, count - totalBytesRead);
            if (bytesRead == 0)
            {
                _sourceStream.Position = 0;
            }
            totalBytesRead += bytesRead;
        }
        return totalBytesRead;
    }
}
