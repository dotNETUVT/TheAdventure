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

            var basePath = AppDomain.CurrentDomain.BaseDirectory; // Gets the base directory of the application
            var relativePath = "Assets\\doom.mp3"; // Relative path to the music file
            var fullPath = Path.Combine(basePath, relativePath); // Combine paths to get the full path

            var musicPlayer = new MusicPlayer(fullPath); // Initialize the music player with the full path
            musicPlayer.Play();

            engine.InitializeWorld();

            bool quit = false;
            while (!quit)
            {
                quit = input.ProcessInput();
                if (quit) break;

                engine.ProcessFrame();
                engine.RenderFrame();
            }

            // Stop music when exiting the game
            musicPlayer.Stop();
            musicPlayer.Dispose();
        }

        sdl.Quit();
    }
}
