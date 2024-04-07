using Silk.NET.SDL;

namespace TheAdventure
{
    /// @brief Manages the creation, rendering, and destruction of the game window.
    /// 
    /// This class encapsulates the functionality provided by SDL for window management. It includes 
    /// methods for creating the game window, creating a renderer for the window, and properly destroying 
    /// the window upon game termination.
    public unsafe class GameWindow
    {
        /// The native window handle provided by SDL.
        private IntPtr _window;
        
        /// The SDL context used for window operations.
        private Sdl _sdl;

        /// @brief Initializes a new instance of the GameWindow class.
        /// 
        /// This constructor creates the game window using specified SDL context. It sets up the window with predefined
        /// dimensions and flags. If the window creation fails, it throws an exception.
        /// 
        /// @param sdl The SDL context to be used for window creation and management.
        public GameWindow(Sdl sdl)
        {
            _sdl = sdl;
            _window = (IntPtr)sdl.CreateWindow(
                "The Adventure", Sdl.WindowposUndefined, Sdl.WindowposUndefined, 800, 800,
                (uint)WindowFlags.Resizable /*| (uint)WindowFlags.AllowHighdpi*/
            );

            // Checks if window creation was successful, otherwise throws an exception
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

        /// @brief Creates a renderer for the window.
        /// 
        /// This method creates an SDL renderer for the window with specified flags. If the renderer creation fails, 
        /// it throws an exception.
        /// 
        /// @return The pointer to the newly created renderer.
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

        /// @brief Destroys the window.
        /// 
        /// This method properly disposes of the SDL window and releases any resources associated with it. 
        /// It should be called when the game is closing or when the window is no longer needed.
        public void Destroy(){
            _sdl.DestroyWindow((Window*)_window);
        }
    }
}