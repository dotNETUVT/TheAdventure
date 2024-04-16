namespace TheAdventure.Models;

using NAudio.Wave;

public class SoundPlayer
{
    private IWavePlayer wavePlayer;
    private LoopStream loopStream;

    public SoundPlayer(string filePath)
    {
        wavePlayer = new WaveOutEvent();
        loopStream = new LoopStream(new AudioFileReader(filePath));
        wavePlayer.Init(loopStream);
    }

    public void Play()
    {
        wavePlayer.Play();
    }

    public void Stop()
    {
        wavePlayer.Stop();
    }

    public void Dispose()
    {
        wavePlayer.Dispose();
        loopStream.Dispose();
    }
}

public class LoopStream : WaveStream
{
    WaveStream sourceStream;

    public LoopStream(WaveStream sourceStream)
    {
        this.sourceStream = sourceStream;
        this.EnableLooping = true;
    }

    public bool EnableLooping { get; set; }

    public override WaveFormat WaveFormat
    {
        get { return sourceStream.WaveFormat; }
    }

    public override long Length
    {
        get { return sourceStream.Length; }
    }

    public override long Position
    {
        get { return sourceStream.Position; }
        set { sourceStream.Position = value; }
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        int totalBytesRead = 0;

        while (totalBytesRead < count)
        {
            int bytesRead = sourceStream.Read(buffer, offset + totalBytesRead, count - totalBytesRead);
            if (bytesRead == 0)
            {
                if (sourceStream.Position == 0 || !EnableLooping)
                {
                    // something wrong with the source stream
                    break;
                }
               
                sourceStream.Position = 0;
            }
            totalBytesRead += bytesRead;
        }
        return totalBytesRead;
    }
}