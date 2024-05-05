using System;
using Silk.NET.SDL;

namespace TheAdventure;

public unsafe class GameWindow : IDisposable
{
    private IntPtr _window;
    private Sdl _sdl;
    private bool _inMainMenu = true;

    public (int Width, int Height) Size
    {
        get
        {
            int width, height;
            _sdl.GetWindowSize((Window *)_window, &width, &height);

            return (width, height);
        }
    }

    public GameWindow(Sdl sdl, int width, int height)
    {
        _sdl = sdl;
        _window = (IntPtr)sdl.CreateWindow(
            "The Adventure", Sdl.WindowposUndefined, Sdl.WindowposUndefined, width, height,
            (uint)WindowFlags.Resizable
        );

        if (_window == IntPtr.Zero)
        {
            var ex = sdl.GetErrorAsException();
            if (ex != null)
            {
                throw ex;
            }

            throw new Exception("Failed to create window.");
        }
            ShowMainMenu();
    }

        private void ShowMainMenu()
        {
            Console.WriteLine("=== Main Menu ===");
            Console.WriteLine("1. Start Game");
            Console.WriteLine("2. Options");
            Console.WriteLine("3. Exit");

            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    _inMainMenu = false;
                    Console.WriteLine("Starting Game...");
                    break;
                case "2":
                    ShowOptionsMenu();
                    break;
                case "3":
                    Environment.Exit(0);
                    break;
                default:
                    Console.WriteLine("Invalid choice. Please enter a valid option.");
                    break;
            }
        }

        private void ShowOptionsMenu()
        {
            Console.WriteLine("=== Options Menu ===");
            Console.WriteLine("1. Change Resolution");
            Console.WriteLine("2. Sound Settings");
            Console.WriteLine("3. Back");

            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    Console.WriteLine("Changing Resolution...");
                    ShowMainMenu();
                    break;
                case "2":
                    Console.WriteLine("Changing Sound Settings...");
                    ShowMainMenu();
                    break;
                case "3":
                    ShowMainMenu();
                    break;
                default:
                    Console.WriteLine("Invalid choice. Please enter a valid option.");
                    ShowMainMenu();
                    break;
            }
        }
        public IntPtr CreateRenderer()
        {
            if (!_inMainMenu)
            {
                var renderer = (IntPtr)_sdl.CreateRenderer((Window*)_window, -1, (uint)RendererFlags.Accelerated);
                if (renderer == IntPtr.Zero)
                {
                    var ex = _sdl.GetErrorAsException();
                    if (ex != null)
                    {
                        throw ex;
                    }

                    throw new Exception("Failed to create renderer.");
                }

                _sdl.RenderSetVSync((Renderer*)renderer, 1);

                return renderer;
            }
            else
            {
                throw new InvalidOperationException("Cannot create renderer while in the main menu.");
            }
        }

    private void ReleaseUnmanagedResources()
    {
        if (_window != IntPtr.Zero)
        {
            _sdl.DestroyWindow((Window*)_window);
            _window = IntPtr.Zero;
        }
    }

    public void Dispose()
    {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    ~GameWindow()
    {
        ReleaseUnmanagedResources();
    }
}