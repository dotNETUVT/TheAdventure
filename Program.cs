using System.Diagnostics;
using Silk.NET.SDL;

namespace TheAdventure;

public static class Program
{
    public static void Main()
    {
        var sdl = new Sdl(new SdlContext());

        var timer = new Stopwatch();

        var sdlInitResult = sdl.Init(Sdl.InitVideo | Sdl.InitEvents);
        if (sdlInitResult < 0)
        {
            throw new InvalidOperationException("Failed to initialize SDL.");
        }

        var gameWindow = new GameWindow(sdl);
        var gameLogic = new GameLogic();
        var gameRenderer = new GameRenderer(sdl, gameWindow, gameLogic);
        var inputLogic = new InputLogic(sdl, gameWindow, gameRenderer, gameLogic);

        gameLogic.InitializeGame(gameRenderer);

        var lastFrameRendereAt = DateTimeOffset.UtcNow;

        bool quit = false;
        while (!quit)
        {
            var timeSinceLastFrame = (int)DateTimeOffset.UtcNow.Subtract(lastFrameRendereAt).TotalMilliseconds;
            
            quit = inputLogic.ProcessInput();
            if(quit) break;
            
            gameLogic.ProcessFrame(timeSinceLastFrame);
            gameRenderer.Render(timeSinceLastFrame);

            lastFrameRendereAt = DateTimeOffset.UtcNow;
        }

        gameWindow.Destroy();

        sdl.Quit();
    }
}