using NAudio.Wave;

namespace TheAdventure
{
    public class Audio
    {
        private WaveOutEvent _waveOut;
        private AudioFileReader _audioFile;

        public Audio()
        {
            _waveOut = new WaveOutEvent();
            _waveOut.PlaybackStopped += OnPlaybackStopped;
        }

        public void OnPlaybackStopped(object sender, StoppedEventArgs e)
        {
            if (_audioFile != null)
            {
                _audioFile.Position = 0;
                _waveOut.Play(); 
            }
        }

        public void LoadBackgroundMusic(string musicFilePath)
        {
            _audioFile = new AudioFileReader(musicFilePath);
            _waveOut.Init(_audioFile);
            _waveOut.Volume = 0.5f; // Set volume between 0.0 to 1.0
        }

        public void PlayBackgroundMusic()
        {
            _waveOut.Play();
        }

        public void StopBackgroundMusic()
        {
            _waveOut.Stop();
        }

     

        public void ResumeBackgroundMusic()
        {
            if (_waveOut.PlaybackState == PlaybackState.Paused)
            {
                _waveOut.Play();
            }
            else
            {
                _waveOut.Pause();
            }
        }
    }
}
