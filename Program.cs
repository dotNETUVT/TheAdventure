using System;
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

        var crowTimer = new Stopwatch();
        crowTimer.Start();

        var sdlInitResult = sdl.Init(Sdl.InitVideo | Sdl.InitEvents | Sdl.InitTimer | Sdl.InitGamecontroller |
                                     Sdl.InitJoystick);
        if (sdlInitResult < 0)
        {
            throw new InvalidOperationException("Failed to initialize SDL.");
        }

        var gameWindow = new GameWindow(sdl);
        var gameLogic = new GameLogic();
        var gameRenderer = new GameRenderer(sdl, gameWindow, gameLogic);
        var inputLogic = new InputLogic(sdl, gameWindow, gameRenderer, gameLogic);

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

            if (crowTimer.Elapsed.TotalSeconds >= 5)
            {
                gameRenderer.RenderCrow();
                crowTimer.Restart();
            }

            System.Threading.Thread.Sleep(TimeSpan.FromSeconds(0.041666666666667));

            if(inputLogic.fatigueTimer.Elapsed.TotalSeconds < inputLogic.fatigueTime)
                gameLogic.setPlayerSpeed(64);
            else if(inputLogic.sprintTimer.IsRunning == false)
                gameLogic.setPlayerSpeed(128);
        }

        gameWindow.Destroy();

        sdl.Quit();
    }
}