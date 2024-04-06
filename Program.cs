using System.Diagnostics;
using Silk.NET.SDL;

namespace TheAdventure;

public static class Program
{
    public static void Main()
    {
        var sdl = new Sdl(new SdlContext());

        ulong framesRenderedCounter = 0;
        var timer = new Stopwatch();

        var sdlInitResult = sdl.Init(Sdl.InitVideo | Sdl.InitEvents | Sdl.InitTimer | Sdl.InitGamecontroller |
                                     Sdl.InitJoystick);
        if (sdlInitResult < 0)
        {
            throw new InvalidOperationException("Failed to initialize SDL.");
        }

        var gameWindow = new GameWindow(sdl);
        var gameLogic = new GameLogic();
        var gameCamera = new GameCamera();
        var gameRenderer = new GameRenderer(sdl, gameWindow, gameLogic, gameCamera);
        var inputLogic = new InputLogic(sdl, gameWindow, gameRenderer, gameLogic, gameCamera);

        gameLogic.LoadGameState();

        bool quit = false;
        while (!quit)
        {
            quit = inputLogic.ProcessInput();
            if(quit) break;
            gameLogic.ProcessFrame();
            
            #region Frame Timer
            var elapsed = timer.Elapsed;
            timer.Restart();
            #endregion

            // game.render(renderer, RenderEvent{ elapsed, framesRenderedCounter++ });
            gameRenderer.Render();

            ++framesRenderedCounter;
            System.Threading.Thread.Sleep(TimeSpan.FromSeconds(0.041666666666667));
        }

        gameWindow.Destroy();

        sdl.Quit();
    }
}