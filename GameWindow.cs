using Silk.NET.SDL;

namespace TheAdventure
{
    public unsafe class GameWindow
    {
        private IntPtr _window;
        private Sdl _sdl;

        public GameWindow(Sdl sdl, int width, int height)
        {
            _sdl = sdl;
            _window = (IntPtr)sdl.CreateWindow(
                "The Adventure", Sdl.WindowposUndefined, Sdl.WindowposUndefined, width, height,
                (uint)WindowFlags.Resizable /*| (uint)WindowFlags.AllowHighdpi*/
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

        public IntPtr CreateRenderer(){

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
            return renderer;
        }

        public void Destroy(){
            _sdl.DestroyWindow((Window*)_window);
        }
    }
}