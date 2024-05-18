using CSCore;
using CSCore.SoundOut;
using CSCore.Codecs;
using CSCore.Streams;
using System;

public class BackgroundSong : IDisposable
{
    private ISoundOut _soundOut;
    private IWaveSource _waveSource;
    private readonly LoopStream _loopStream;
    private readonly string _filePath;
    private float _volume;

    public BackgroundSong(string filePath, bool loop = false, float initialVolume = 1.0f)
    {
        _filePath = filePath;
        _waveSource = CodecFactory.Instance.GetCodec(filePath)
            .ToSampleSource()
            .ToWaveSource();

        _loopStream = new LoopStream(_waveSource) { EnableLoop = loop };
        _volume = initialVolume;
        _soundOut = new WasapiOut();
        _soundOut.Initialize(_loopStream);
        SetVolume(_volume);
    }

    public void Play()
    {
        if (_soundOut.PlaybackState != PlaybackState.Playing)
        {
            _soundOut.Play();
        }
    }

    public void Pause()
    {
        if (_soundOut.PlaybackState == PlaybackState.Playing)
        {
            _soundOut.Pause();
        }
    }

    public void Stop()
    {
        _soundOut.Stop();
    }

    public void ToggleLoop(bool enable)
    {
        _loopStream.EnableLoop = enable;
    }

    public void SetVolume(float volume)
    {
        _volume = Math.Clamp(volume, 0.0f, 1.0f);
        _soundOut.Volume = _volume;
    }

    public void Restart()
    {
        Stop();
        _waveSource.Dispose();
        _waveSource = CodecFactory.Instance.GetCodec(_filePath)
            .ToSampleSource()
            .ToWaveSource();
        _loopStream.WaveSource = _waveSource;
        _soundOut.Initialize(_loopStream);
        SetVolume(_volume);
        Play();
    }

    public void Dispose()
    {
        _soundOut.Dispose();
        _waveSource.Dispose();
    }

    private class LoopStream : WaveAggregatorBase
    {
        public bool EnableLoop { get; set; }

        public LoopStream(IWaveSource source) : base(source) { }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (!EnableLoop)
                return base.Read(buffer, offset, count);

            int totalBytesRead = 0;

            while (totalBytesRead < count)
            {
                int bytesRead = base.Read(buffer, offset + totalBytesRead, count - totalBytesRead);
                if (bytesRead == 0)
                {
                    if (Source.CanSeek)
                    {
                        Source.Position = 0;
                    }
                    else
                    {
                        break;
                    }
                }
                totalBytesRead += bytesRead;
            }

            return totalBytesRead;
        }
    }
}