using System;
using System.Collections.Generic;
using System.IO;
using System.Media;

namespace TheAdventure
{
    public class SoundEffectManager : IDisposable
    {
        private readonly Dictionary<string, string> _soundEffectFiles;
        private SoundPlayer _soundPlayer;

        public SoundEffectManager()
        {
            _soundEffectFiles = new Dictionary<string, string>();
            _soundPlayer = new SoundPlayer();
        }

        public void LoadSoundEffect(string name, string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Sound effect file not found: {filePath}");
            }

            _soundEffectFiles[name] = filePath;
        }

        public void PlaySoundEffect(string name)
        {
            if (_soundEffectFiles.TryGetValue(name, out var filePath))
            {
                if (_soundPlayer != null)
                {
                    _soundPlayer.Stop();
                    _soundPlayer.SoundLocation = filePath;
                    _soundPlayer.Load();
                    _soundPlayer.Play();
                }
            }
            else
            {
                throw new KeyNotFoundException($"Sound effect '{name}' not found.");
            }
        }

        public void Dispose()
        {
            if (_soundPlayer != null)
            {
                _soundPlayer.Dispose();
                _soundPlayer = null;
            }
        }
    }
}
