// Explanation of Changes:
/*
 * 1. Animation Map: Introduced a dictionary _animationMap to store animation names based on player states and directions, simplifying animation handling.
 * 2. Code Organization: Moved animation map initialization to a separate method InitializeAnimationMap() for clarity and maintainability.
 * 3. Attack Direction Handling: Introduced a method GetDirection to determine the attack direction based on boolean parameters, improving readability and reducing redundancy.
 * 4. Position Update Optimization: Simplified position update logic in the UpdatePlayerPosition method, enhancing code readability and maintainability.
 * 5. Idle State Check: Introduced a method IsIdle to check if the player is in an idle state, improving code readability.
 * 6. Boundary Clamping: Used Math.Clamp to ensure the player stays within the game boundaries, improving robustness and preventing potential bugs.
 */
using System;
using System.Collections.Generic;
using Silk.NET.Maths;

namespace TheAdventure.Models
{
    // The PlayerObject class represents the player character in the game.
    public class PlayerObject : RenderableGameObject
    {
        // Enumerations to define player states and directions.
        public enum PlayerStateDirection
        {
            None = 0,
            Down,
            Up,
            Left,
            Right,
        }

        public enum PlayerState
        {
            None = 0,
            Idle,
            Move,
            Attack,
            GameOver
        }

        private int _pixelsPerSecond = 192;

        // A dictionary to map each combination of PlayerState and PlayerStateDirection to an animation name.
        private readonly Dictionary<(PlayerState, PlayerStateDirection), string> _animationMap =
            new Dictionary<(PlayerState, PlayerStateDirection), string>();

        // Property to get or set the current state of the player.
        public (PlayerState State, PlayerStateDirection Direction) State { get; private set; }

        // Constructor to initialize the player object with a sprite sheet and position.
        public PlayerObject(SpriteSheet spriteSheet, int x, int y) : base(spriteSheet, (x, y))
        {
            InitializeAnimationMap(); // Initialize the animation map.
            SetState(PlayerState.Idle, PlayerStateDirection.Down); // Set initial state to Idle.
        }

        // Method to initialize the animation map.
        private void InitializeAnimationMap()
        {
            // Loop through all possible combinations of PlayerState and PlayerStateDirection.
            foreach (PlayerState state in Enum.GetValues(typeof(PlayerState)))
            {
                foreach (PlayerStateDirection direction in Enum.GetValues(typeof(PlayerStateDirection)))
                {
                    // Assign animation names based on states and directions.
                    if (state == PlayerState.None || direction == PlayerStateDirection.None)
                    {
                        _animationMap[(state, direction)] = null;
                    }
                    else if (state == PlayerState.GameOver)
                    {
                        _animationMap[(state, direction)] = Enum.GetName(state);
                    }
                    else
                    {
                        _animationMap[(state, direction)] = Enum.GetName(state) + Enum.GetName(direction);
                    }
                }
            }
        }

        // Method to set the state of the player.
        public void SetState(PlayerState state, PlayerStateDirection direction)
        {
            if (State.State == PlayerState.GameOver || State.State == state && State.Direction == direction)
            {
                return;
            }

            // Activate the corresponding animation based on the state and direction.
            string animationName = _animationMap[(state, direction)];
            SpriteSheet.ActivateAnimation(animationName);
            State = (state, direction);
        }

        // Method to handle the game over state.
        public void GameOver()
        {
            SetState(PlayerState.GameOver, PlayerStateDirection.None);
        }

        // Method to initiate an attack action based on direction inputs.
        public void Attack(bool up, bool down, bool left, bool right)
        {
            if (State.State == PlayerState.GameOver)
            {
                return;
            }

            // Determine the direction of the attack based on input parameters.
            PlayerStateDirection direction = GetDirection(up, down, left, right);
            SetState(PlayerState.Attack, direction);
        }

        // Method to get the direction based on input parameters.
        private PlayerStateDirection GetDirection(bool up, bool down, bool left, bool right)
        {
            if (up)
            {
                return PlayerStateDirection.Up;
            }
            if (down)
            {
                return PlayerStateDirection.Down;
            }
            if (right)
            {
                return PlayerStateDirection.Right;
            }
            if (left)
            {
                return PlayerStateDirection.Left;
            }
            return PlayerStateDirection.None; // Default direction
        }

        // Method to update the player's position based on movement inputs.
        public void UpdatePlayerPosition(double up, double down, double left, double right, int width, int height,
            double time)
        {
            if (State.State == PlayerState.GameOver || IsIdle(up, down, left, right))
            {
                return;
            }

            double pixelsToMove = time * _pixelsPerSecond;

            int x = GetUpdatedPosition(Position.X, left, right, width, pixelsToMove);
            int y = GetUpdatedPosition(Position.Y, up, down, height, pixelsToMove); // Corrected order of up and down

            // Set the new position of the player.
            Position = (x, y);
        }

        // Method to check if the player is in the idle state.
        private bool IsIdle(double up, double down, double left, double right)
        {
            return up <= double.Epsilon && down <= double.Epsilon && left <= double.Epsilon && right <= double.Epsilon;
        }

        // Method to calculate the updated position based on movement inputs.
        private int GetUpdatedPosition(int currentPosition, double negativeDirection, double positiveDirection,
            int boundary, double pixelsToMove)
        {
            int newPosition = currentPosition + (int)(positiveDirection * pixelsToMove);
            newPosition -= (int)(negativeDirection * pixelsToMove);
            return Math.Clamp(newPosition, 10, boundary - 10); // Ensure the player stays within the game boundaries.
        }
    }
}