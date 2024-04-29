using System.Diagnostics;

using Silk.NET.SDL;
using System;
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
            var basePath = AppDomain.CurrentDomain.BaseDirectory; // Retrieves the root directory of the application.
            var relPath = "Assets\\travis_fein.mp3"; // The path relative to the location of the music file.
            var fullPath = Path.Combine(basePath, relPath); 

            var musicz = new Musicz(fullPath); // Initialize the music player using the complete file path.
            musicz.Start(); 


            engine.InitializeWorld();

            bool quit = false;
            while (!quit)
            {
                quit = input.ProcessInput();
                if (quit) break;
                
                if (input.IsSpacePressed())
                {
                    musicz.TogglePlayPause();
                }
                engine.ProcessFrame();
                engine.RenderFrame();
            }
            
            musicz.Halt();
            musicz.Release();
        }

        sdl.Quit();
    }
    
}