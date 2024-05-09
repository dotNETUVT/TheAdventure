using NAudio.Wave;
using System;

namespace TheAdventure
{
    public class BackgroundMusic : IDisposable
    {
        private readonly string _filePath;
        private WaveOutEvent _waveOut;
        private AudioFileReader _audioFileReader;

        public BackgroundMusic(string filePath)
        {
            _filePath = filePath;
            _waveOut = new WaveOutEvent();
            _audioFileReader = new AudioFileReader(filePath);
            _waveOut.Init(_audioFileReader);
            _waveOut.Volume = 0.5f;
        }

        public void Play()
        {
            _waveOut.Play();
        }

        public void Stop()
        {
            _waveOut.Stop();
        }

        public void Dispose()
        {
            _waveOut.Dispose();
            _audioFileReader.Dispose();
        }
    }
}
