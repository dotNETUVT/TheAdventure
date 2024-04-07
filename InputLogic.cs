using Silk.NET.SDL;

namespace TheAdventure{
    /// @brief Manages input handling for the game, including keyboard and mouse events.
    /// 
    /// This class uses the Silk.NET.SDL library to process input events and update the game state accordingly. It supports
    /// keyboard and mouse input, translating these inputs into game actions such as moving the player or placing bombs.
    public unsafe class InputLogic
    {
        /// The SDL context for handling input.
        private Sdl _sdl;
        
        /// The game logic instance to update based on input.
        private GameLogic _gameLogic;
        
        /// The game window instance for managing input focus.
        private GameWindow _gameWindow;
        
        /// The renderer for potentially updating visual feedback based on input.
        private GameRenderer _renderer;
        
        /// The timestamp of the last input update.
        private DateTimeOffset _lastUpdate;

        /// @brief Initializes a new instance of the InputLogic class with necessary game components.
        /// 
        /// @param sdl The SDL context for handling input.
        /// @param window The game window for managing input focus.
        /// @param renderer The game renderer for updating visuals based on input.
        /// @param logic The game logic for updating game state based on input.
        public InputLogic(Sdl sdl, GameWindow window, GameRenderer renderer, GameLogic logic){
            _sdl = sdl;
            _gameLogic = logic;
            _gameWindow = window;
            _renderer = renderer;
            _lastUpdate = DateTimeOffset.UtcNow;
        }

        /// @brief Processes all pending input events and updates the game state accordingly.
        /// 
        /// This method polls for SDL events and handles them, translating keyboard and mouse inputs into game actions. It updates
        /// the game state through the GameLogic instance and tracks the time since the last input update for time-sensitive actions.
        /// 
        /// @return True if a quit event is received, indicating the game should close; false otherwise.
        public bool ProcessInput()
        {
            var currentTime = DateTimeOffset.UtcNow;
            ReadOnlySpan<byte> _keyboardState = new(_sdl.GetKeyboardState(null), (int)KeyCode.Count);
            Span<byte> mouseButtonStates = stackalloc byte[(int)MouseButton.Count];
            Event ev = new Event();
            var mouseX = 0;
            var mouseY = 0;
            while (_sdl.PollEvent(ref ev) != 0)
            {
                if (ev.Type == (uint)EventType.Quit)
                {
                    return true;
                }

                switch (ev.Type)
                {
                    case (uint)EventType.Windowevent:
                    {
                        switch (ev.Window.Event)
                        {
                            case (byte)WindowEventID.Shown:
                            case (byte)WindowEventID.Exposed:
                            {
                                break;
                            }
                            case (byte)WindowEventID.Hidden:
                            {
                                break;
                            }
                            case (byte)WindowEventID.Moved:
                            {
                                break;
                            }
                            case (byte)WindowEventID.SizeChanged:
                            {
                                break;
                            }
                            case (byte)WindowEventID.Minimized:
                            case (byte)WindowEventID.Maximized:
                            case (byte)WindowEventID.Restored:
                                break;
                            case (byte)WindowEventID.Enter:
                            {
                                break;
                            }
                            case (byte)WindowEventID.Leave:
                            {
                                break;
                            }
                            case (byte)WindowEventID.FocusGained:
                            {
                                break;
                            }
                            case (byte)WindowEventID.FocusLost:
                            {
                                break;
                            }
                            case (byte)WindowEventID.Close:
                            {
                                break;
                            }
                            case (byte)WindowEventID.TakeFocus:
                            {
                                unsafe
                                {
                                    _sdl.SetWindowInputFocus(_sdl.GetWindowFromID(ev.Window.WindowID));
                                }

                                break;
                            }
                        }

                        break;
                    }

                    case (uint)EventType.Fingermotion:
                    {
                        break;
                    }

                    case (uint)EventType.Mousemotion:
                    {
                        break;
                    }

                    case (uint)EventType.Fingerdown:
                    {
                        mouseButtonStates[(byte)MouseButton.Primary] = 1;
                        break;
                    }
                    case (uint)EventType.Mousebuttondown:
                    {
                        mouseX = ev.Motion.X;
                        mouseY = ev.Motion.Y;
                        mouseButtonStates[ev.Button.Button] = 1;
                        break;
                    }

                    case (uint)EventType.Fingerup:
                    {
                        mouseButtonStates[(byte)MouseButton.Primary] = 0;
                        break;
                    }

                    case (uint)EventType.Mousebuttonup:
                    {
                        mouseButtonStates[ev.Button.Button] = 0;
                        break;
                    }

                    case (uint)EventType.Mousewheel:
                    {
                        break;
                    }

                    case (uint)EventType.Keyup:
                    {
                        
                        break;
                    }

                    case (uint)EventType.Keydown:
                    {
                        break;
                    }
                }
            }

            var timeSinceLastUpdateInMS = (int)currentTime.Subtract(_lastUpdate).TotalMilliseconds;

            if (_keyboardState[(int)Scancode.ScancodeUp] == 1){
                _gameLogic.UpdatePlayerPosition(1.0, 0, 0, 0, timeSinceLastUpdateInMS);
            }
            else if (_keyboardState[(int)Scancode.ScancodeDown] == 1){
                _gameLogic.UpdatePlayerPosition(0, 1.0, 0, 0, timeSinceLastUpdateInMS);
            }
            else if (_keyboardState[(int)Scancode.ScancodeLeft] == 1){
                _gameLogic.UpdatePlayerPosition(0, 0, 1.0, 0, timeSinceLastUpdateInMS);
            }
            else if (_keyboardState[(int)Scancode.ScancodeRight] == 1){
                _gameLogic.UpdatePlayerPosition(0, 0, 0, 1.0, timeSinceLastUpdateInMS);
            }

            _lastUpdate = currentTime;

            if (mouseButtonStates[(byte)MouseButton.Primary] == 1){
                _gameLogic.AddBomb(mouseX, mouseY);
            }
            return false;
        }
    }
}