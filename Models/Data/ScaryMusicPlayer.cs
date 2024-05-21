using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Wave;
using System;

namespace TheAdventure.Models;
public class Audio 
{
    private IWavePlayer backgroundMusicPlayer;
    private IWavePlayer deathSoundPlayer;
    private AudioFileReader backgroundMusic;
    private AudioFileReader deathSound;

    public void LoadBackgroundMusic(string filePath)
    {
        backgroundMusic = new AudioFileReader(filePath);
        backgroundMusicPlayer = new WaveOutEvent();
        backgroundMusicPlayer.Init(backgroundMusic);
    }

    public void LoadDeathSound(string filePath)
    {
        deathSound = new AudioFileReader(filePath);
        deathSoundPlayer = new WaveOutEvent();
        deathSoundPlayer.Init(deathSound);
    }

    public void PlayBackgroundMusic()
    {
        backgroundMusicPlayer.Play();
    }

    public void StopBackgroundMusic()
    {
        backgroundMusicPlayer.Stop();
    }

    public void PlayDeathSound()
    {
        deathSoundPlayer.Play();
    }

}