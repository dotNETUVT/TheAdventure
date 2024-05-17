using Silk.NET.SDL;

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
    }

    public IntPtr CreateRenderer()
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