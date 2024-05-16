using NAudio.Wave;

namespace TheAdventure.Models
{
    public class SoundManager
    {
        private readonly string _soundFilePath;
        private IWavePlayer _waveOut;
        private WaveStream _waveStream;

        public SoundManager(string soundFilePath)
        {
            _soundFilePath = soundFilePath;
        }

        public void Play(bool loop = false)
        {
            try
            {
                Stop();

                _waveOut = new WaveOutEvent();
                _waveStream = new AudioFileReader(_soundFilePath);

                if (loop)
                {
                    _waveStream = new LoopStream(_waveStream);
                }

                _waveOut.Init(_waveStream);
                _waveOut.Play();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error playing sound: {ex.Message}");
            }
        }

        public void Stop()
        {
            _waveOut?.Stop();
            _waveOut?.Dispose();
            _waveStream?.Dispose();
            _waveOut = null;
            _waveStream = null;
        }

        private class LoopStream : WaveStream
        {
            private readonly WaveStream _sourceStream;

            public LoopStream(WaveStream sourceStream)
            {
                _sourceStream = sourceStream;
                EnableLooping = true;
            }

            public bool EnableLooping { get; set; }

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
                        if (_sourceStream.Position == 0 || !EnableLooping)
                        {
                            break;
                        }
                        // Loop
                        _sourceStream.Position = 0;
                    }
                    totalBytesRead += bytesRead;
                }
                return totalBytesRead;
            }
        }
    }
}
