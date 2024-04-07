using Silk.NET.SDL;

namespace TheAdventure{
    public unsafe class InputLogic
    {
        private Sdl _sdl;
        private GameLogic _gameLogic;
        private GameWindow _gameWindow;
        private GameRenderer _renderer;
        private DateTimeOffset _lastUpdate;

        public InputLogic(Sdl sdl, GameWindow window, GameRenderer renderer, GameLogic logic){
            _sdl = sdl;
            _gameLogic = logic;
            _gameWindow = window;
            _renderer = renderer;
            _lastUpdate = DateTimeOffset.UtcNow;
        }

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
                                var width = ev.Window.Data1;
                                var height = ev.Window.Data2;
                                _renderer.ResizeCamera(width, height);
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

           
            //Support for diagonal movement
            _gameLogic.UpdatePlayerPosition(_keyboardState[(int)Scancode.ScancodeUp],
                (double)_keyboardState[(int)Scancode.ScancodeDown],
                (double)_keyboardState[(int)Scancode.ScancodeLeft],
                (double)_keyboardState[(int)Scancode.ScancodeRight],
                timeSinceLastUpdateInMS);

            _lastUpdate = currentTime;

            if (mouseButtonStates[(byte)MouseButton.Primary] == 1){
                _gameLogic.AddBomb(mouseX, mouseY);
            }
            return false;
        }
    }
}