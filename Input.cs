using Silk.NET.SDL;
using TheAdventure.Models;

namespace TheAdventure
{
    public unsafe class Input
    {
        private Sdl _sdl;
        private GameWindow _gameWindow;
        private Engine _gameEngine;
        private GameRenderer _renderer;
        private PlayerObject _player;
        bool isSpacebarPressed = false;
        private bool _isSprinting = false; // Flag to track sprint mode

        byte[] _mouseButtonStates = new byte[(int)MouseButton.Count];

        public EventHandler<(int x, int y)> OnMouseClick;

        public Input(Sdl sdl, GameWindow window, GameRenderer renderer, PlayerObject player)
        {
            _sdl = sdl;
            _gameWindow = window;
            _renderer = renderer;
            _player = player; // Add this line to store the PlayerObject instance
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
        public bool IsRPressed()
        {
            ReadOnlySpan<byte> _keyboardState = new(_sdl.GetKeyboardState(null), (int)KeyCode.Count);
            return _keyboardState[(int)KeyCode.R] == 1;
        }
        public bool IsSpacePressed()
        {
            ReadOnlySpan<byte> keyboardState = new(_sdl.GetKeyboardState(null), (int)KeyCode.Count);
            return keyboardState[(int)KeyCode.Space] == 1;
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
                            // Handle window events
                            break;
                        }

                    case (uint)EventType.Fingermotion:
                        {
                            // Handle finger motion events
                            break;
                        }

                    case (uint)EventType.Mousemotion:
                        {
                            // Handle mouse motion events
                            break;
                        }

                    case (uint)EventType.Fingerdown:
                        {
                            // Handle finger down events
                            _mouseButtonStates[(byte)MouseButton.Primary] = 1;
                            break;
                        }

                    case (uint)EventType.Mousebuttondown:
                        {
                            // Handle mouse button down events
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
                            // Handle finger up events
                            _mouseButtonStates[(byte)MouseButton.Primary] = 0;
                            break;
                        }

                    case (uint)EventType.Mousebuttonup:
                        {
                            // Handle mouse button up events
                            _mouseButtonStates[ev.Button.Button] = 0;
                            break;
                        }

                    case (uint)EventType.Mousewheel:
                        {
                            // Handle mouse wheel events
                            break;
                        }

                    case (uint)EventType.Keyup:
                        {
                            // Handle key up events
                            break;
                        }

                    case (uint)EventType.Keydown:
                        {
                            // Handle key down events
                            if (ev.Key.Keysym.Sym == (int)KeyCode.R)
                            {
                                // Toggle sprint mode
                                _isSprinting = !_isSprinting;

                                // Set sprint mode for the player
                                _player.SetSprint(_isSprinting);
                            }
                            else if (ev.Key.Keysym.Sym == (int)KeyCode.Space)
                            {
                                // Set the flag when spacebar is pressed
                                isSpacebarPressed = true;
                            }

                            break;
                        }
                }
            }

            return false;
        }
    }
}
