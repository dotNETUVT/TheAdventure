using Silk.NET.SDL;

namespace TheAdventure
{
    public unsafe class Input
    {
        // list with the original Konami code, to compare with the user input
        private List<KeyCode> konamiCode = new List<KeyCode>
        {
            KeyCode.Up, KeyCode.Up,
            KeyCode.Down, KeyCode.Down,
            KeyCode.Left, KeyCode.Right,
            KeyCode.Left, KeyCode.Right,
            KeyCode.KpZero, KeyCode.KpNine // B, A Keys 
        };
        // list to save the keys pressed by the user
        private List<KeyCode> pressedKeys = new List<KeyCode>();

        // event to be triggered when the Konami code is entered
        public event EventHandler OnKonamiCode;
        // flag to check if the Konami code was entered
        private Boolean _konamiCodeEntered = false;
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


        private bool CheckKonamiCode()
        {   
            // if the user makes a mistake, clear the list of pressed keys and return false
            int keyIndex = pressedKeys.Count - 1;
            if(pressedKeys[keyIndex] != konamiCode[keyIndex])
            {   
                pressedKeys.Clear();
                return false;
            }

            // if length doesnt match return false
            if (pressedKeys.Count < konamiCode.Count)
            {   
                Console.WriteLine($"Pressed keys: {pressedKeys.Count} - Konami code: {konamiCode.Count}");
                return false;
            }

            // verify if the keys pressed by the user match the Konami code
            for (int i = 0; i < konamiCode.Count; i++)
            {   

                if (pressedKeys[i] != konamiCode[i])
                {   
                    Console.WriteLine();
                    // empty the list of pressed keys if the user makes a mistake
                    pressedKeys.Clear();
                    // return false if the user makes a mistake
                    return false;
                }

                Console.Write($"Key {i} matches: {pressedKeys[i]} ");
            }

            // if all keys were pressed in the correct order, empty the list of pressed keys
            // and return true
            pressedKeys.Clear();
            _konamiCodeEntered = true;
            return true;
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
                        
                        if (_konamiCodeEntered)
                        {
                            break;
                        }

                        // get the pressed key
                        KeyCode pressedKey = (KeyCode) ev.Key.Keysym.Sym;

                        pressedKeys.Add(pressedKey);

                        Console.WriteLine($"Key pressed: {pressedKey} ");

                        if(CheckKonamiCode())
                        {   
                            // trigger the event if the Konami code was entered correctly
                            OnKonamiCode?.Invoke(this, EventArgs.Empty);
                            
                            // reset the event
                            OnKonamiCode = null;
                        }

                        break;
                    }
                }
            }

            return false;
        }
        public Boolean IsKonamiCodeEntered()
        {
            return _konamiCodeEntered;
        }
    }
}