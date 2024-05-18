using CSCore;
using CSCore.SoundOut;
using CSCore.Codecs;
using System;
using System.Collections.Generic;

public class BackgroundSong : IDisposable
{
    private ISoundOut _soundOut;
    private IWaveSource _waveSource;
    private LoopStream _loopStream;
    private List<string> _filePaths;
    private int _currentTrackIndex;
    private float _volume;
    private bool _loop;

    public BackgroundSong(List<string> filePaths, bool loop = false, float initialVolume = 1.0f)
    {
        _filePaths = filePaths;
        _currentTrackIndex = 0;
        _volume = initialVolume;
        _loop = loop;
        LoadTrack(_currentTrackIndex);
    }

    private void LoadTrack(int trackIndex)
    {
        DisposeWaveSource();
        _waveSource = CodecFactory.Instance.GetCodec(_filePaths[trackIndex])
            .ToSampleSource()
            .ToWaveSource();

        _loopStream = new LoopStream(_waveSource) { EnableLoop = _loop };
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
        _loop = enable;
    }

    public void SetVolume(float volume)
    {
        _volume = Math.Clamp(volume, 0.0f, 1.0f);
        _soundOut.Volume = _volume;
    }

    public void ChangeTrack()
    {
        _currentTrackIndex = (_currentTrackIndex + 1) % _filePaths.Count;
        LoadTrack(_currentTrackIndex);
        Play();
    }

    public void LoadTrackByPath(string path)
    {
        DisposeWaveSource();
        _waveSource = CodecFactory.Instance.GetCodec(path)
            .ToSampleSource()
            .ToWaveSource();

        _loopStream = new LoopStream(_waveSource) { EnableLoop = _loop };
        _soundOut.Initialize(_loopStream);
        SetVolume(_volume);
        Play();
    }

    public void Dispose()
    {
        DisposeWaveSource();
        _soundOut?.Dispose();
    }

    private void DisposeWaveSource()
    {
        _waveSource?.Dispose();
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
