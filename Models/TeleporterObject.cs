using Silk.NET.Maths;
using TheAdventure;
using System;

namespace TheAdventure.Models
{
    public class TeleporterObject : RenderableGameObject
    {
        public enum TeleporterState
        {
            Idle,
            Activated,
            Cooldown
        }

        public TeleporterState State { get; private set; }

        public TeleporterObject(SpriteSheet spriteSheet, int x, int y) : base(spriteSheet, (x, y))
        {
            SetState(TeleporterState.Idle);
        }

        public void SetState(TeleporterState state)
        {
            if (State == state)
            {
                return;
            }

            if (state == TeleporterState.Idle)
            {
                SpriteSheet.ActivateAnimation("Idle");
            }
            else if (state == TeleporterState.Activated)
            {
                SpriteSheet.ActivateAnimation("Activate");               
                State = TeleporterState.Activated;
                return;
            }
            else if (state == TeleporterState.Cooldown)
            {
                SpriteSheet.ActivateAnimation("Cooldown");
            }

            State = state;
        }

        public void Activate()
        {
            SetState(TeleporterState.Activated);
        }

        public void Deactivate()
        {
            SetState(TeleporterState.Idle);
        }

        public bool IsAvailable()
        {
            //if (State == TeleporterState.Cooldown)
            //{
            //    Deactivate();
            //}

            return State == TeleporterState.Idle;
        }
    }
}
