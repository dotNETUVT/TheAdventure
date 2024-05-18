using System;
using TheAdventure.Models;

namespace TheAdventure.Models
{
    public class Octopus : RenderableGameObject
    {
        private readonly PlayerObject _player;
        private readonly Engine _engine;
        private readonly double _attackInterval = 5;
        private double _timeSinceLastAttack;
        private readonly int _attackRange = 50;
        private bool _isAttacking;
        private bool _isAttackAnimationFinished;
        public bool _isGameOver = false;
        private readonly Random _random;

        public Octopus(SpriteSheet spriteSheet, int x, int y, PlayerObject player, Engine engine) 
            : base(spriteSheet, (x, y))
        {
            _player = player;
            _engine = engine;
            _random = new Random();
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
            SpawnBomb();
        }

        private void SpawnBomb()
        {
            var offsetX = _random.Next(-_attackRange, _attackRange);
            var offsetY = _random.Next(-_attackRange, _attackRange);

            var bombX = Position.X + offsetX;
            var bombY = Position.Y + offsetY;

            _engine.AddBomb(bombX, bombY, false);
        }

        public void GameOver()
        {
            _isGameOver = true;
            SpriteSheet.ActivateAnimation("GameOver");
        }
    }
}
