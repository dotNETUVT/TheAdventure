using System;
using TheAdventure.Models;

namespace TheAdventure.Models
{
    public class Zombie : RenderableGameObject
    {
        private readonly PlayerObject _player;
        private readonly double _attackInterval = 2.0;
        private double _timeSinceLastAttack;
        private bool _isAttacking;
        private bool _isAttackAnimationFinished;
        private readonly int _attackRange = 30;
        public bool _isGameOver;

        public Zombie(SpriteSheet spriteSheet, int x, int y, PlayerObject player) 
            : base(spriteSheet, (x, y))
        {
            _player = player;
        }

        public void Update(double deltaTime)
        {
            if (_isGameOver) return;
            _timeSinceLastAttack += deltaTime;
            if (_timeSinceLastAttack >= _attackInterval && !_isAttacking)
            {
                _isAttacking = true;
                _isAttackAnimationFinished = false;
                Attack();
                _timeSinceLastAttack = 0;
            }

            if (_isAttacking && _isAttackAnimationFinished)
            {
                _isAttacking = false;
                SpriteSheet.StopAnimation();
            }

            if (_isAttacking && SpriteSheet.ActiveAnimation != null)
            {
                var totalFrames = (SpriteSheet.ActiveAnimation.EndFrame.Row - SpriteSheet.ActiveAnimation.StartFrame.Row) * SpriteSheet.ColumnCount +
                                  SpriteSheet.ActiveAnimation.EndFrame.Col - SpriteSheet.ActiveAnimation.StartFrame.Col + 1;
                var currentFrame = (int)((DateTimeOffset.Now - SpriteSheet._animationStart).TotalMilliseconds /
                                         (SpriteSheet.ActiveAnimation.DurationMs / totalFrames));
                if (currentFrame >= totalFrames)
                {
                    _isAttackAnimationFinished = true;
                }
            }
        }

        private void Attack()
        {
            SpriteSheet.ActivateAnimation("Attack");

            if (IsPlayerInRange())
            {
                _player.GameOver();
            }
        }

        private bool IsPlayerInRange()
        {
            return Math.Abs(Position.X - _player.Position.X) < _attackRange &&
                   Math.Abs(Position.Y - _player.Position.Y) < _attackRange;
        }

        public void GameOver()
        {
            _isGameOver = true;
            SpriteSheet.ActivateAnimation("GameOver");
        }
    }
}
