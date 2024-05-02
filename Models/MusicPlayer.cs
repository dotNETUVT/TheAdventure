using NAudio.Wave;
using System;
using System.Threading.Tasks;

public class MusicPlayer : IDisposable
{
    private readonly WaveOutEvent _outputDevice;
    private AudioFileReader _audioFile;
    private BufferedWaveProvider _bufferedWaveProvider;
    private float _volume = 0.5f;
    private bool _isPlaying;
    private DateTime _startTime;

    public MusicPlayer(string filePath)
    {
        _outputDevice = new WaveOutEvent();
        LoadMusic(filePath);
    }

    private void LoadMusic(string filePath)
    {
        _audioFile?.Dispose();
        _audioFile = new AudioFileReader(filePath) { Volume = _volume };

        _bufferedWaveProvider = new BufferedWaveProvider(_audioFile.WaveFormat);
        _bufferedWaveProvider.BufferDuration = TimeSpan.FromSeconds(5); 

        _outputDevice.Init(_bufferedWaveProvider);
    }

    public async Task PlayMusicAsync()
    {
        if (!_isPlaying)
        {
            _outputDevice.Play();
            _isPlaying = true;
            _startTime = DateTime.Now;

            await Task.Run(ReadAudioAsync);
        }
    }

    private async Task ReadAudioAsync()
    {
        while (_isPlaying)
        {
            byte[] buffer = new byte[9000]; 
            int bytesRead = await _audioFile.ReadAsync(buffer, 0, buffer.Length);
            if (bytesRead == 0)
            {
                _audioFile.Position = 0;
            }
            else
            {
                _bufferedWaveProvider.AddSamples(buffer, 0, bytesRead);
            }
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
            PlayMusicAsync().GetAwaiter();
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

    public bool IsMusicPlaying => _isPlaying;

    public DateTime StartTime => _startTime;

    public void Dispose()
    {
        _outputDevice.Dispose();
        _audioFile.Dispose();
    }
}
