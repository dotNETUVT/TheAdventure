using NAudio.Wave;

public class Soundtrack
{
    private WaveOutEvent _waveOutDevice;
    private AudioFileReader _audioFile;
    private float _volumeLevel = 0.4f; 
    private bool _isPlaying;

    public Soundtrack(string filePath)
    {
        _waveOutDevice = new WaveOutEvent(); 
        LoadAudioFile(filePath); 
    }

    private void LoadAudioFile(string filePath)
    {
        _audioFile?.Dispose(); 
        _audioFile = new AudioFileReader(filePath) { Volume = _volumeLevel }; 

        
        var loopStream = new LoopStream(_audioFile);
        _waveOutDevice.Init(loopStream); 
    }

    public void Play()
    {
        if (!_isPlaying)
        {
            _waveOutDevice.Play(); // Start audio playback
            _isPlaying = true;
        }
    }

    public void Stop()
    {
        if (_isPlaying)
        {
            _waveOutDevice.Stop(); // Stop audio playback
            _isPlaying = false;
        }
    }

    public void TogglePlayPause()
    {
        if (_isPlaying)
        {
            Stop(); // If currently playing, stop the playback
        }
        else
        {
            Play(); // If not playing, start the playback
        }
    }
}

// Custom stream class to loop audio playback
public class LoopStream : WaveStream
{
    private readonly WaveStream _sourceStream;

    public LoopStream(WaveStream source)
    {
        _sourceStream = source ?? throw new ArgumentNullException(nameof(source)); 
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
            int bytesRead = _sourceStream.Read(buffer, offset + totalBytesRead, count - totalBytesRead); // Read from source
            if (bytesRead == 0)
            {
                _sourceStream.Position = 0; 
            }
            totalBytesRead += bytesRead;
        }
        return totalBytesRead; 
    }
}
