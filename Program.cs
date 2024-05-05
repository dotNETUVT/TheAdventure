using System;
using Silk.NET.SDL;
using TheAdventure.Models;

namespace TheAdventure
{
    public static class Program
    {
        public static void Main()
        {
            var sdl = new Sdl(new SdlContext());

            var sdlInitResult = sdl.Init(Sdl.InitVideo | Sdl.InitAudio);
            if (sdlInitResult < 0)
            {
                throw new InvalidOperationException("Failed to initialize SDL.");
            }

            using (var window = new GameWindow(sdl, 960, 640))
            {
                var renderer = new GameRenderer(sdl, window);

                // Load the sprite sheet
                var spriteSheet = SpriteSheet.LoadSpriteSheet("player.json", "Assets", renderer);

                // Check if the sprite sheet was loaded successfully
                if (spriteSheet == null)
                {
                    throw new InvalidOperationException("Failed to load the player sprite sheet.");
                }

                // Create an instance of PlayerObject with the sprite sheet and initial position
                var player = new PlayerObject(spriteSheet, 100, 100);

                var input = new Input(sdl, window, renderer, player); // Pass the player object to the Input constructor
                var engine = new Engine(renderer, input);

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
}
