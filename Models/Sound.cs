using NAudio.Wave;
using System;
using System.Collections.Generic;

namespace TheAdventure.Models
{
    public class Sound
    {
        private readonly Dictionary<string, (IWavePlayer, AudioFileReader)> _sounds = new Dictionary<string, (IWavePlayer, AudioFileReader)>();

        public void LoadSound(string soundName, string filePath)
        {
            try
            {
                var audioFileReader = new AudioFileReader(filePath);
                var waveOutEvent = new WaveOutEvent();
                waveOutEvent.Init(audioFileReader);
                _sounds[soundName] = (waveOutEvent, audioFileReader);
                Console.WriteLine($"Sound '{soundName}' loaded successfully from '{filePath}'.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading sound '{soundName}' from '{filePath}': {ex.Message}");
            }
        }

        public void PlaySound(string soundName)
        {
            if (_sounds.TryGetValue(soundName, out var soundPlayer))
            {
                try
                {
                    soundPlayer.Item1.Play();
                    Console.WriteLine($"Playing sound '{soundName}'.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error playing sound '{soundName}': {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine($"Sound '{soundName}' not found.");
            }
        }
        
        public void StopSound(string soundName)
        {
            if (_sounds.TryGetValue(soundName, out var soundPlayer))
            {
                try
                {
                    soundPlayer.Item1.Stop();
                    Console.WriteLine($"Stopped sound '{soundName}'.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error stopping sound '{soundName}': {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine($"Sound '{soundName}' not found.");
            }
        }
    }
}