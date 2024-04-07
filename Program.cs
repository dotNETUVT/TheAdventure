using System.Diagnostics;
using Silk.NET.SDL;

namespace TheAdventure;

/// @class Program
/// @brief Main entry point and control hub for "The Adventure" game.
///
/// This class initializes the game environment, including graphics, input handling, and game logic components.
/// It also contains the main game loop, which processes input, updates game state, and renders frames.
public static class Program
{
    /// @fn Main
    /// @brief Initializes the game and enters the main game loop.
    ///
    /// This method sets up the SDL environment, creates the game window, and initializes all major components
    /// of the game. It enters a loop that continues until the game receives a quit event. Within this loop,
    /// it processes user input, updates game logic, and renders frames at a fixed interval.
    public static void Main()
    {
        // Initialize the SDL context for handling low-level operations like window management and event handling.
        var sdl = new Sdl(new SdlContext());

        // Counter for the number of frames rendered.
        ulong framesRenderedCounter = 0;
        var timer = new Stopwatch(); // Stopwatch for frame timing.

        // Initialize SDL with video, events, timers, game controller, and joystick support.
        var sdlInitResult = sdl.Init(Sdl.InitVideo | Sdl.InitEvents | Sdl.InitTimer | Sdl.InitGamecontroller |
                                     Sdl.InitJoystick);
        if (sdlInitResult < 0)
        {
            throw new InvalidOperationException("Failed to initialize SDL.");
        }

        // Create main game window, logic, renderer, and input logic instances.
        var gameWindow = new GameWindow(sdl);
        var gameLogic = new GameLogic();
        var gameRenderer = new GameRenderer(sdl, gameWindow, gameLogic);
        var inputLogic = new InputLogic(sdl, gameWindow, gameRenderer, gameLogic);

        // Load initial game state (levels, assets, etc.).
        gameLogic.LoadGameState();

        bool quit = false;
        
        // Main game loop: process input, update logic, render frame, and manage frame timing.
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

        gameWindow.Destroy(); // Clean up and close the game window.

        sdl.Quit();
    }
}