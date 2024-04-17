using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMPLib;

namespace TheAdventure
{
    public class MusicPlayer
    {
        private WindowsMediaPlayer _player;

        public MusicPlayer(string filepath)
        {
            _player = new WindowsMediaPlayer();
            _player.URL = filepath;
            _player.settings.setMode("loop", true);
            _player.settings.volume = 50;
            Console.WriteLine($"Initialized music player with file: {filepath}");
        }

        public void Play()
        {
            try
            {
                _player.controls.play();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error trying to play: {ex.Message}");
            }

            if (_player.settings.mute)
            {
                _player.settings.mute = false;
            }
            _player.controls.play();
        }

        public bool IsMuted
        {
            get => _player.settings.mute;
            set => _player.settings.mute = value;
        }

        public void Stop()
        {
            _player.controls.stop();
        }

        public void Pause()
        {
            _player.controls.pause();
        }

        public void Resume()
        {
            _player.controls.play();
        }

        public bool IsPlaying
        {
            get
                {
                return _player.playState == WMPPlayState.wmppsPlaying;
            }
        }
    }
}
