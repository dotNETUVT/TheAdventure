using Silk.NET.SDL;

namespace TheAdventure
{
    public unsafe class Input
    {
        private Sdl _sdl;
        private GameWindow _gameWindow;
        private GameRenderer _renderer;
        
        byte[] _mouseButtonStates = new byte[(int)MouseButton.Count];
        
        public EventHandler<(int x, int y)> OnMouseClick;
        
        public Input(Sdl sdl, GameWindow window, GameRenderer renderer)
        {
            _sdl = sdl;
            _gameWindow = window;
            _renderer = renderer;
        }

        public bool IsLeftPressed()
        {
            ReadOnlySpan<byte> _keyboardState = new(_sdl.GetKeyboardState(null), (int)KeyCode.Count);
            return _keyboardState[(int)KeyCode.Left] == 1;
        }
        
        public bool IsRightPressed()
        {
            ReadOnlySpan<byte> _keyboardState = new(_sdl.GetKeyboardState(null), (int)KeyCode.Count);
            return _keyboardState[(int)KeyCode.Right] == 1;
        }
        
        public bool IsUpPressed()
        {
            ReadOnlySpan<byte> _keyboardState = new(_sdl.GetKeyboardState(null), (int)KeyCode.Count);
            return _keyboardState[(int)KeyCode.Up] == 1;
        }
        
        public bool IsDownPressed()
        {
            ReadOnlySpan<byte> _keyboardState = new(_sdl.GetKeyboardState(null), (int)KeyCode.Count);
            return _keyboardState[(int)KeyCode.Down] == 1;
        }

        public bool IsAPressed()
        {
            ReadOnlySpan<byte> _keyboardState = new(_sdl.GetKeyboardState(null), (int)KeyCode.Count);
            return _keyboardState[(int)KeyCode.A] == 1;
        }
        
        public bool IsDPressed()
        {
            ReadOnlySpan<byte> _keyboardState = new(_sdl.GetKeyboardState(null), (int)KeyCode.Count);
            return _keyboardState[(int)KeyCode.D] == 1;
        }
        
        public bool IsWPressed()
        {
            ReadOnlySpan<byte> _keyboardState = new(_sdl.GetKeyboardState(null), (int)KeyCode.Count);
            return _keyboardState[(int)KeyCode.W] == 1;
        }
        
        public bool IsSPressed()
        {
            ReadOnlySpan<byte> _keyboardState = new(_sdl.GetKeyboardState(null), (int)KeyCode.Count);
            return _keyboardState[(int)KeyCode.S] == 1;
        }

        public bool IsFPressed()
        {
            ReadOnlySpan<byte> _keyboardState = new(_sdl.GetKeyboardState(null), (int)KeyCode.Count);
            return _keyboardState[(int)KeyCode.F] == 1;
        }

        public bool IsKPressed()
        {
            ReadOnlySpan<byte> _keyboardState = new(_sdl.GetKeyboardState(null), (int)KeyCode.Count);
            return _keyboardState[(int)KeyCode.K] == 1;
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
                            OnMouseClick?.Invoke(this, (mouseX, mouseY));
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
                        break;
                    }

                    case (uint)EventType.Keydown:
                    {
                        break;
                    }
                }
            }

            return false;
        }
    }
}