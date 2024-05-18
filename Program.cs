using System.Diagnostics;
using Silk.NET.SDL;
using System.IO;
using System.Collections.Generic;

namespace TheAdventure
{
    public static class Program
    {
        public static void Main()
        {
            var sdl = new Sdl(new SdlContext());

            var sdlInitResult = sdl.Init(Sdl.InitVideo | Sdl.InitEvents | Sdl.InitTimer | Sdl.InitGamecontroller | Sdl.InitJoystick);
            if (sdlInitResult < 0)
            {
                throw new InvalidOperationException("Failed to initialize SDL.");
            }

            using (var window = new GameWindow(sdl, 800, 480))
            {
                var renderer = new GameRenderer(sdl, window);
                var input = new Input(sdl, window, renderer);
                var engine = new Engine(renderer, input);

                var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                var soundPath = Path.Combine(baseDirectory, "Assets\\backgroundSound.mp3");
                var backgroundSound2Path = Path.Combine(baseDirectory, "Assets\\backgroundSound2.mp3");

                var tracks = new List<string> { soundPath, backgroundSound2Path };
                var backgroundSong = new BackgroundSong(tracks, loop: true);
                backgroundSong.Play();

                engine.InitializeWorld();

                bool quit = false;
                while (!quit)
                {
                    quit = input.ProcessInput();
                    if (quit) break;

                    if (sdl.PollEvent(out var e) == 1 && e.Type == Sdl.EventType.KeyDown)
                    {
                        if (e.Key.Keysym.Sym == Sdl.Keycode.C)
                        {
                            backgroundSong.ChangeTrack();
                        }
                    }

                    engine.ProcessFrame();
                    engine.RenderFrame();
                }

                backgroundSong.Dispose();
            }

            sdl.Quit();
        }
    }
}
