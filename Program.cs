using System;
using System.Diagnostics;
using Silk.NET.SDL;
using System.Runtime.InteropServices;

namespace TheAdventure
{
    public static class Program
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool AllocConsole();

        public static void Main()
        {
            AllocConsole();
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

                engine.InitializeWorld();

                bool quit = false;
                while (!quit)
                {
                    quit = input.ProcessInput();
                    if (quit) break;

                    if (input.IsEscJustPressed())
                    {
                        engine.TogglePause();
                        renderer.TogglePauseButtonDisplay(engine.IsPaused);
                    }

                    if (!engine.IsPaused)
                    {
                        engine.ProcessFrame();
                        engine.RenderFrame();
                    }
                }
            }

            sdl.Quit();
        }
    }
}
