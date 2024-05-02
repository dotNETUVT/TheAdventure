using System;
using System.Runtime.InteropServices;
using Silk.NET.SDL;

namespace TheAdventure;

public static class Program
{
    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool AllocConsole();
    public static void Main()
    {
        AllocConsole();
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
                if (input.IsSpaceJustPressed())
                {
                    engine.TogglePause();
                    Console.WriteLine($"Paused: {engine.IsPaused()}");  // Should log the current pause state
                }

                if (!engine.IsPaused())
                {
                    engine.ProcessFrame();
                    engine.RenderFrame();
                }

                // Process other events or input
                quit = input.ProcessInput();
            }

            // Stop music when exiting the game
            musicPlayer.Stop();
            musicPlayer.Dispose();
        }

        sdl.Quit();
    }
}
