using Silk.NET.SDL;

namespace TheAdventure
{
    public unsafe class Input
    {
        private Sdl _sdl;
        private GameWindow _gameWindow;
        private GameRenderer _renderer;

        byte[] _mouseButtonStates = new byte[(int)MouseButton.Count];

        int[] _mouseCoords = new int[2];

        bool[] directionPresses = new bool[4];

        public EventHandler<(int x, int y)> OnLeftMouseClick;
        public EventHandler<(int x, int y)> OnRightMouseClick;
        public EventHandler OnUseButton;
        public EventHandler<int> NewDirectionKey;
        public EventHandler<bool> AllMovementOff;

        public Input(Sdl sdl, GameWindow window, GameRenderer renderer)
        {
            _sdl = sdl;
            _gameWindow = window;
            _renderer = renderer;
        }

        public bool IsLeftPressed()
        {
            ReadOnlySpan<byte> _keyboardState = new(_sdl.GetKeyboardState(null), (int)KeyCode.Count);
            return _keyboardState[(int)KeyCode.A] == 1;
        }

        public bool IsRightPressed()
        {
            ReadOnlySpan<byte> _keyboardState = new(_sdl.GetKeyboardState(null), (int)KeyCode.Count);
            return _keyboardState[(int)KeyCode.D] == 1;
        }

        public bool IsUpPressed()
        {
            ReadOnlySpan<byte> _keyboardState = new(_sdl.GetKeyboardState(null), (int)KeyCode.Count);
            return _keyboardState[(int)KeyCode.W] == 1;
        }

        public bool IsDownPressed()
        {
            ReadOnlySpan<byte> _keyboardState = new(_sdl.GetKeyboardState(null), (int)KeyCode.Count);
            return _keyboardState[(int)KeyCode.S] == 1;
        }


        public bool ProcessInput()
        {
            var currentTime = DateTimeOffset.UtcNow;
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
                        _mouseCoords[0] = ev.Motion.X;
                        _mouseCoords[1] = ev.Motion.Y;
                        break;
                    }

                    case (uint)EventType.Fingerdown:
                    {
                        _mouseButtonStates[(byte)MouseButton.Primary] = 1;
                        break;
                    }

                    case (uint)EventType.Mousebuttondown:
                    {
                        mouseX = ev.Motion.X;
                        mouseY = ev.Motion.Y;
                        _mouseButtonStates[ev.Button.Button] = 1;

                        if (ev.Button.Button == (byte)MouseButton.Primary)
                        {
                            OnLeftMouseClick?.Invoke(this, (mouseX, mouseY));
                        }

                        if (ev.Button.Button == (byte)MouseButton.Secondary)
                        {
                            OnRightMouseClick?.Invoke(this, (mouseX, mouseY));
                        }

                        break;
                    }

                    case (uint)EventType.Fingerup:
                    {
                        _mouseButtonStates[(byte)MouseButton.Primary] = 0;
                        break;
                    }

                    case (uint)EventType.Mousebuttonup:
                    {
                        _mouseButtonStates[ev.Button.Button] = 0;
                        break;
                    }

                    case (uint)EventType.Mousewheel:
                    {
                        break;
                    }

                    case (uint)EventType.Keyup:
                    {
                        if (ev.Key.Keysym.Scancode == Scancode.ScancodeW)
                        {
                            directionPresses[0] = false;
                        }

                        if (ev.Key.Keysym.Scancode == Scancode.ScancodeA)
                        {
                            directionPresses[3] = false;
                        }

                        if (ev.Key.Keysym.Scancode == Scancode.ScancodeS)
                        {
                            directionPresses[2] = false;
                        }

                        if (ev.Key.Keysym.Scancode == Scancode.ScancodeD)
                        {
                            directionPresses[1] = false;
                        }

                        //manual aggregate cause for some reason it didnt work as expected
                        bool moving = false;
                        for (int i = 0; i < 4; i++)
                        {
                            moving = moving || directionPresses[i];
                        }

                        if (!moving)
                            AllMovementOff.Invoke(this, true);
                        break;
                    }

                    case (uint)EventType.Keydown:
                    {
                        if (ev.Key.Repeat == 0)
                        {
                            if (ev.Key.Keysym.Scancode == Scancode.ScancodeE)
                            {
                                OnUseButton.Invoke(this, EventArgs.Empty);
                            }

                            if (ev.Key.Keysym.Scancode == Scancode.ScancodeW)
                            {
                                directionPresses[0] = true;
                                NewDirectionKey.Invoke(this, 0);
                            }

                            if (ev.Key.Keysym.Scancode == Scancode.ScancodeA)
                            {
                                directionPresses[3] = true;
                                NewDirectionKey.Invoke(this, 3);
                            }

                            if (ev.Key.Keysym.Scancode == Scancode.ScancodeS)
                            {
                                directionPresses[2] = true;
                                NewDirectionKey.Invoke(this, 2);
                            }

                            if (ev.Key.Keysym.Scancode == Scancode.ScancodeD)
                            {
                                directionPresses[1] = true;
                                NewDirectionKey.Invoke(this, 1);
                            }
                        }

                        break;
                    }
                }
            }

            return false;
        }
    }
}