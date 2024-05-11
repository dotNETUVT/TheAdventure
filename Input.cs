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

        private Dictionary<int, IntPtr> _gameControllers = new Dictionary<int, IntPtr>();
        
        public Input(Sdl sdl, GameWindow window, GameRenderer renderer)
        {
            _sdl = sdl;
            _gameWindow = window;
            _renderer = renderer;
        }

        public void InitializeControllers()
        {
            int numJoysticks = _sdl.NumJoysticks();
            Console.WriteLine($"Number of joysticks detected: {numJoysticks}");
            for (int i = 0; i < numJoysticks; i++)
            {
                if (_sdl.IsGameController(i) == SdlBool.True)
                {
                    IntPtr controller = (IntPtr)_sdl.GameControllerOpen(i);
                    if (controller != IntPtr.Zero)
                    {
                        _gameControllers[i] = controller;
                        Console.WriteLine($"Game controller {i} is connected and initialized.");
                    }
                    else
                    {
                        Console.WriteLine($"Game controller {i} is connected but failed to initialize.");
                    }
                }
            }
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

        public bool IsWPressed()
        {
            ReadOnlySpan<byte> _keyboardState = new(_sdl.GetKeyboardState(null), (int)KeyCode.Count);
            return _keyboardState[(int)KeyCode.W] == 1;
        }

        public bool IsAPressed()
        {
            ReadOnlySpan<byte> _keyboardState = new(_sdl.GetKeyboardState(null), (int)KeyCode.Count);
            return _keyboardState[(int)KeyCode.A] == 1;
        }

        public bool IsSPressed()
        {
            ReadOnlySpan<byte> _keyboardState = new(_sdl.GetKeyboardState(null), (int)KeyCode.Count);
            return _keyboardState[(int)KeyCode.S] == 1;
        }

        public bool IsDPressed()
        {
            ReadOnlySpan<byte> _keyboardState = new(_sdl.GetKeyboardState(null), (int)KeyCode.Count);
            return _keyboardState[(int)KeyCode.D] == 1;
        }

        public bool IsJoystickLeftPressed()
        {
            var controller = _gameControllers.FirstOrDefault().Value;
            if (controller != IntPtr.Zero)
            {
                unsafe
                {
                    GameController* controllerPtr = (GameController*)controller;
                    int axisValue = _sdl.GameControllerGetAxis(controllerPtr, GameControllerAxis.Leftx);
                    return axisValue < -8000;
                }
            }
            return false;
        }

        public bool IsJoystickRightPressed()
        {
            var controller = _gameControllers.FirstOrDefault().Value;
            if (controller != IntPtr.Zero)
            {
                unsafe
                {
                    GameController* controllerPtr = (GameController*)controller;
                    int axisValue = _sdl.GameControllerGetAxis(controllerPtr, GameControllerAxis.Leftx);
                    return axisValue > 8000;
                }
            }
            return false;
        }

        public bool IsJoystickUpPressed()
        {
            var controller = _gameControllers.FirstOrDefault().Value;
            if (controller != IntPtr.Zero)
            {
                unsafe
                {
                    GameController* controllerPtr = (GameController*)controller;
                    int axisValue = _sdl.GameControllerGetAxis(controllerPtr, GameControllerAxis.Lefty);
                    return axisValue < -8000;
                }
            }
            return false;
        }

        public bool IsJoystickDownPressed()
        {
            var controller = _gameControllers.FirstOrDefault().Value;
            if (controller != IntPtr.Zero)
            {
                unsafe
                {
                    GameController* controllerPtr = (GameController*)controller;
                    int axisValue = _sdl.GameControllerGetAxis(controllerPtr, GameControllerAxis.Lefty);
                    return axisValue > 8000;
                }
            }
            return false;
        }

        public bool IsAButtonPressed()
        {
            var controller = _gameControllers.FirstOrDefault().Value;
            if (controller != IntPtr.Zero)
            {
                unsafe
                {
                    GameController* controllerPtr = (GameController*)controller;
                    return _sdl.GameControllerGetButton(controllerPtr, GameControllerButton.A) == 1;
                }
            }
            return false;
        }

        public bool IsBButtonPressed()
        {
            var controller = _gameControllers.FirstOrDefault().Value;
            if (controller != IntPtr.Zero)
            {
                unsafe
                {
                    GameController* controllerPtr = (GameController*)controller;
                    return _sdl.GameControllerGetButton(controllerPtr, GameControllerButton.B) == 1;
                }
            }
            return false;
        }

        public bool IsXButtonPressed()
        {
            var controller = _gameControllers.FirstOrDefault().Value;
            if (controller != IntPtr.Zero)
            {
                unsafe
                {
                    GameController* controllerPtr = (GameController*)controller;
                    return _sdl.GameControllerGetButton(controllerPtr, GameControllerButton.X) == 1;
                }
            }
            return false;
        }

        public bool IsYButtonPressed()
        {
            var controller = _gameControllers.FirstOrDefault().Value;
            if (controller != IntPtr.Zero)
            {
                unsafe
                {
                    GameController* controllerPtr = (GameController*)controller;
                    return _sdl.GameControllerGetButton(controllerPtr, GameControllerButton.Y) == 1;
                }
            }
            return false;
        }

        private void CleanupControllers()
        {
            foreach (var controller in _gameControllers.Values)
            {
                if (controller != IntPtr.Zero)
                {
                    _sdl.GameControllerClose((GameController*)controller);
                }
            }
            _gameControllers.Clear();
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
                    CleanupControllers();
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