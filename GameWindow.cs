using Silk.NET.SDL;
using System;

namespace TheAdventure;

public unsafe class GameWindow : IDisposable
{
    private IntPtr _window;
    private Sdl _sdl;

    public (int Width, int Height) Size
    {
        get
        {
            int width, height;
            _sdl.GetWindowSize((Window*)_window, &width, &height);
            return (width, height);
        }
    }

    public GameWindow(Sdl sdl, int width, int height, bool startFullscreen = false)
    {
        _sdl = sdl;
        uint flags = (uint)(WindowFlags.Resizable | (startFullscreen ? WindowFlags.Fullscreen : 0));
        _window = (IntPtr)_sdl.CreateWindow(
            "The Adventure", Sdl.WindowposUndefined, Sdl.WindowposUndefined, width, height, flags
        );

        if (_window == IntPtr.Zero)
        {
            var ex = _sdl.GetErrorAsException();
            throw ex ?? new Exception("Failed to create window.");
        }
    }

    public IntPtr CreateRenderer(bool vsync = true, bool accelerated = true)
    {
        uint rendererFlags = (uint)(RendererFlags.Software); 

        if (accelerated)
        {
            rendererFlags = (uint)(RendererFlags.Accelerated);
        }


        var renderer = (IntPtr)_sdl.CreateRenderer((Window*)_window, -1, rendererFlags);
        if (renderer == IntPtr.Zero)
        {
            var ex = _sdl.GetErrorAsException();
            throw ex ?? new Exception("Failed to create renderer.");
        }

        _sdl.RenderSetVSync((Renderer*)renderer, vsync ? 1 : 0);
        return renderer;
    }


    public void ToggleFullscreen()
    {
        uint flags = (uint)WindowFlags.Fullscreen;
        uint currentFlags = _sdl.GetWindowFlags((Window*)_window);
        _sdl.SetWindowFullscreen((Window*)_window, (currentFlags & flags) == flags ? 0 : flags);
    }

    public void SetWindowSize(int width, int height)
    {
        _sdl.SetWindowSize((Window*)_window, width, height);
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
