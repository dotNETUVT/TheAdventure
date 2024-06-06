using System.Diagnostics;
using Silk.NET.SDL;
using NAudio;
using NAudio.Wave;
namespace TheAdventure;

public static class Program
{
    public static void Main()
    {
        var sdl = new Sdl(new SdlContext());

        var sdlInitResult = sdl.Init(Sdl.InitVideo | Sdl.InitEvents | Sdl.InitTimer | Sdl.InitGamecontroller |
                                     Sdl.InitJoystick);
        if (sdlInitResult < 0)
        {
            throw new InvalidOperationException("Failed to initialize SDL.");
        }

        using (var window = new GameWindow(sdl, 800, 480))
        {
            var renderer = new GameRenderer(sdl, window);
            var input = new Input(sdl, window, renderer);
            var engine = new Engine(renderer, input);

            WaveOutEvent backgroundMusic = new WaveOutEvent();
            AudioFileReader audioFile = new AudioFileReader("C:\\Users\\katam\\Source\\Repos\\MelnicKatalin\\TheAdventure\\Assets\\backgroundMusic.wav");
            backgroundMusic.Init(audioFile);
            backgroundMusic.Play();
            engine.InitializeWorld();

            bool quit = false;
            while (!quit)
            {
                quit = input.ProcessInput();
                if (quit) break;
                
                engine.ProcessFrame();
                engine.RenderFrame();
            }
        }

        sdl.Quit();
    }
}