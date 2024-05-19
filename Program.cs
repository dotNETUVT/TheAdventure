using System.Diagnostics;
using Silk.NET.SDL;

namespace TheAdventure
{
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

            try
            {
                using (var window = new GameWindow(sdl, 800, 480))
                {
                    var renderer = new GameRenderer(sdl, window);
                    var input = new Input(sdl, window, renderer);
                    var engine = new Engine(renderer, input);

                    engine.InitializeWorld();

                    bool quit = false;
                    const int fps = 60;
                    var frameDuration = TimeSpan.FromSeconds(1.0 / fps);
                    var stopwatch = Stopwatch.StartNew();

                    while (!quit)
                    {
                        quit = input.ProcessInput();
                        if (quit) break;

                        engine.ProcessFrame();
                        engine.RenderFrame();

                        var elapsed = stopwatch.Elapsed;
                        if (elapsed < frameDuration)
                        {
                            var sleepTime = frameDuration - elapsed;
                            System.Threading.Thread.Sleep(sleepTime);
                        }
                        stopwatch.Restart();
                    }
                }
            }
            finally
            {
                sdl.Quit();
            }
        }
    }
}