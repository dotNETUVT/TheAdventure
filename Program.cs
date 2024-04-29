using System;
using Silk.NET.SDL;

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
            var pathToRepo = AppDomain.CurrentDomain.BaseDirectory; // Gets the base directory of the application
            var pathToSound = "Assets\\music.mp3"; // Relative path to the music file
            var pathCombined = Path.Combine(pathToRepo, pathToSound); // Combine paths to get the full path

            var soundeffect = new SoundEffects(pathCombined); // Initialize the music player with the full path
            soundeffect.Play();

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