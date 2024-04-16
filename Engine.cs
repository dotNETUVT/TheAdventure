using System.Text.Json;
using Silk.NET.Maths;
using Silk.NET.SDL;
using TheAdventure.Models;

namespace TheAdventure
{
    public class Engine
    {
        private readonly Dictionary<int, GameObject> _gameObjects = new();
        private readonly Dictionary<string, TileSet> _loadedTileSets = new();

        private Level? _currentLevel;
        private PlayerObject _player;
        private GameRenderer _renderer;
        private Input _input;

        private DateTimeOffset _lastUpdate = DateTimeOffset.Now;
        private DateTimeOffset _lastPlayerUpdate = DateTimeOffset.Now;

        public Engine(GameRenderer renderer, Input input)
        {
            _renderer = renderer;
            _input = input;

            _input.OnUseButton += (_, _) => AddBomb();
            _input.OnLeftMouseClick += (_, coords) => PlayerSlashAnimation(coords.x, coords.y);
            _input.NewDirectionKey += (_, direction) => PlayerMovementAnimation(direction);
            _input.AllMovementOff += (_, idle) => PlayerIdleAnimation(idle);
        }

        public void InitializeWorld()
        {
            var jsonSerializerOptions = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };
            var levelContent = File.ReadAllText(Path.Combine("Assets", "terrain.tmj"));

            var level = JsonSerializer.Deserialize<Level>(levelContent, jsonSerializerOptions);
            if (level == null) return;
            foreach (var refTileSet in level.TileSets)
            {
                var tileSetContent = File.ReadAllText(Path.Combine("Assets", refTileSet.Source));
                if (!_loadedTileSets.TryGetValue(refTileSet.Source, out var tileSet))
                {
                    tileSet = JsonSerializer.Deserialize<TileSet>(tileSetContent, jsonSerializerOptions);

                    foreach (var tile in tileSet.Tiles)
                    {
                        var internalTextureId = _renderer.LoadTexture(Path.Combine("Assets", tile.Image), out _);
                        tile.InternalTextureId = internalTextureId;
                    }

                    _loadedTileSets[refTileSet.Source] = tileSet;
                }

                refTileSet.Set = tileSet;
            }

            _currentLevel = level;
            SpriteSheet spriteSheet = new(_renderer, Path.Combine("Assets", "player.png"), 10, 6, 48, 48, (24, 42));
            spriteSheet.Animations["IdleDown"] = new SpriteSheet.Animation()
            {
                StartFrame = (0, 0),
                EndFrame = (0, 5),
                DurationMs = 1000,
                Loop = true
            };
            spriteSheet.Animations["IdleUp"] = new SpriteSheet.Animation()
            {
                StartFrame = (2, 0),
                EndFrame = (2, 5),
                DurationMs = 1000,
                Loop = true
            };
            spriteSheet.Animations["IdleRight"] = new SpriteSheet.Animation()
            {
                StartFrame = (1, 0),
                EndFrame = (1, 5),
                DurationMs = 1000,
                Loop = true
            };
            spriteSheet.Animations["IdleLeft"] = new SpriteSheet.Animation()
            {
                StartFrame = (1, 0),
                EndFrame = (1, 5),
                DurationMs = 1000,
                Loop = true,
                Flip = RendererFlip.Horizontal
            };

            spriteSheet.Animations["SlashUp"] = new SpriteSheet.Animation()
            {
                StartFrame = (8, 0),
                EndFrame = (8, 3),
                DurationMs = 300,
                Loop = false,
                Flip = RendererFlip.None
            };

            spriteSheet.Animations["SlashDown"] = new SpriteSheet.Animation()
            {
                StartFrame = (6, 0),
                EndFrame = (6, 3),
                DurationMs = 300,
                Loop = false,
                Flip = RendererFlip.None
            };

            spriteSheet.Animations["SlashRight"] = new SpriteSheet.Animation()
            {
                StartFrame = (7, 0),
                EndFrame = (7, 3),
                DurationMs = 300,
                Loop = false,
                Flip = RendererFlip.None
            };

            spriteSheet.Animations["SlashLeft"] = new SpriteSheet.Animation()
            {
                StartFrame = (7, 0),
                EndFrame = (7, 3),
                DurationMs = 300,
                Loop = false,
                Flip = RendererFlip.Horizontal
            };

            spriteSheet.Animations["WalkUp"] = new SpriteSheet.Animation()
            {
                StartFrame = (5, 0),
                EndFrame = (5, 5),
                DurationMs = 300,
                Loop = true,
                Flip = RendererFlip.None
            };
            spriteSheet.Animations["WalkDown"] = new SpriteSheet.Animation()
            {
                StartFrame = (3, 0),
                EndFrame = (3, 5),
                DurationMs = 300,
                Loop = true,
                Flip = RendererFlip.None
            };
            spriteSheet.Animations["WalkRight"] = new SpriteSheet.Animation()
            {
                StartFrame = (4, 0),
                EndFrame = (4, 5),
                DurationMs = 300,
                Loop = true,
                Flip = RendererFlip.None
            };
            spriteSheet.Animations["WalkLeft"] = new SpriteSheet.Animation()
            {
                StartFrame = (4, 0),
                EndFrame = (4, 5),
                DurationMs = 300,
                Loop = true,
                Flip = RendererFlip.Horizontal
            };


            _player = new PlayerObject(spriteSheet, 100, 100);

            _renderer.SetWorldBounds(new Rectangle<int>(0, 0, _currentLevel.Width * _currentLevel.TileWidth,
                _currentLevel.Height * _currentLevel.TileHeight));
        }

        public void ProcessFrame()
        {
            var currentTime = DateTimeOffset.Now;
            var secsSinceLastFrame = (currentTime - _lastUpdate).TotalSeconds;
            _lastUpdate = currentTime;

            bool up = _input.IsUpPressed();
            bool down = _input.IsDownPressed();
            bool left = _input.IsLeftPressed();
            bool right = _input.IsRightPressed();


            _player.UpdatePlayerPosition(up ? 1.0 : 0.0, down ? 1.0 : 0.0, left ? 1.0 : 0.0, right ? 1.0 : 0.0,
                _currentLevel.Width * _currentLevel.TileWidth, _currentLevel.Height * _currentLevel.TileHeight,
                secsSinceLastFrame);

            var itemsToRemove = new List<int>();
            itemsToRemove.AddRange(GetAllTemporaryGameObjects().Where(gameObject => gameObject.IsExpired)
                .Select(gameObject => gameObject.Id).ToList());

            foreach (var gameObject in itemsToRemove)
            {
                _gameObjects.Remove(gameObject);
            }
        }

        public void RenderFrame()
        {
            _renderer.SetDrawColor(0, 0, 0, 255);
            _renderer.ClearScreen();

            _renderer.CameraLookAt(_player.Position.X, _player.Position.Y);

            RenderTerrain();
            RenderAllObjects();

            _renderer.PresentFrame();
        }

        private Tile? GetTile(int id)
        {
            if (_currentLevel == null) return null;
            foreach (var tileSet in _currentLevel.TileSets)
            {
                foreach (var tile in tileSet.Set.Tiles)
                {
                    if (tile.Id == id)
                    {
                        return tile;
                    }
                }
            }

            return null;
        }

        private void RenderTerrain()
        {
            if (_currentLevel == null) return;
            for (var layer = 0; layer < _currentLevel.Layers.Length; ++layer)
            {
                var cLayer = _currentLevel.Layers[layer];

                for (var i = 0; i < _currentLevel.Width; ++i)
                {
                    for (var j = 0; j < _currentLevel.Height; ++j)
                    {
                        var cTileId = cLayer.Data[j * cLayer.Width + i] - 1;
                        var cTile = GetTile(cTileId);
                        if (cTile == null) continue;

                        var src = new Rectangle<int>(0, 0, cTile.ImageWidth, cTile.ImageHeight);
                        var dst = new Rectangle<int>(i * cTile.ImageWidth, j * cTile.ImageHeight, cTile.ImageWidth,
                            cTile.ImageHeight);

                        _renderer.RenderTexture(cTile.InternalTextureId, src, dst);
                    }
                }
            }
        }

        private IEnumerable<RenderableGameObject> GetAllRenderableObjects()
        {
            foreach (var gameObject in _gameObjects.Values)
            {
                if (gameObject is RenderableGameObject renderableGameObject)
                {
                    yield return renderableGameObject;
                }
            }
        }

        private IEnumerable<TemporaryGameObject> GetAllTemporaryGameObjects()
        {
            foreach (var gameObject in _gameObjects.Values)
            {
                if (gameObject is TemporaryGameObject temporaryGameObject)
                {
                    yield return temporaryGameObject;
                }
            }
        }

        private void RenderAllObjects()
        {
            foreach (var gameObject in GetAllRenderableObjects())
            {
                gameObject.Render(_renderer);
            }

            _player.Render(_renderer);
        }

        private void AddBomb()
        {
            SpriteSheet spriteSheet = new(_renderer, "BombExploding.png", 1, 13, 32, 64, (16, 48));
            spriteSheet.Animations["Explode"] = new SpriteSheet.Animation()
            {
                StartFrame = (0, 0),
                EndFrame = (0, 12),
                DurationMs = 2000,
                Loop = false
            };
            spriteSheet.ActivateAnimation("Explode");
            TemporaryGameObject bomb = new(spriteSheet, 2.1, (_player.Position.X, _player.Position.Y));
            _gameObjects.Add(bomb.Id, bomb);
        }

        private void PlayerSlashAnimation(int x, int y)
        {
            
            var mouseWorldX = _renderer.TranslateFromScreenToWorldCoordinates(x, y).X;
            var mouseWorldY = _renderer.TranslateFromScreenToWorldCoordinates(x, y).Y;
            double Angle = Math.Acos(1.0 *
                                     (mouseWorldX - _player.Position.X) /
                                     Math.Sqrt(
                                         Math.Pow(mouseWorldX - _player.Position.X, 2) +
                                         Math.Pow(mouseWorldY - _player.Position.Y, 2)));
            if (mouseWorldY > _player.Position.Y)
            {
                if (Angle > Math.PI / 4 && Angle <= 3 * Math.PI / 4)
                    _player.SpriteSheet.ActivateAnimation("SlashDown");
                if (Angle > 3 * Math.PI / 4)
                    _player.SpriteSheet.ActivateAnimation("SlashLeft");
                if (Angle <= Math.PI / 4)
                    _player.SpriteSheet.ActivateAnimation("SlashRight");
            }
            else
            {
                if (Angle > Math.PI / 4 && Angle <= 3 * Math.PI / 4)
                    _player.SpriteSheet.ActivateAnimation("SlashUp");
                if (Angle > 3 * Math.PI / 4)
                    _player.SpriteSheet.ActivateAnimation("SlashLeft");
                if (Angle <= Math.PI / 4)
                    _player.SpriteSheet.ActivateAnimation("SlashRight");
            }
        }

        private void PlayerMovementAnimation(int direction)
        {
            // Console.Write("playerMovementAnim: \n");
            // Console.Write("lastDir " + _player.lastDirection + "\n");
            // Console.Write("dir " + direction + "\n");
            switch (direction)
            {
                case 0:
                    _player.SpriteSheet.ActivateAnimation("WalkUp");
                    break;
                case 1:
                    _player.SpriteSheet.ActivateAnimation("WalkRight");
                    break;
                case 2:
                    _player.SpriteSheet.ActivateAnimation("WalkDown");
                    break;
                case 3:
                    _player.SpriteSheet.ActivateAnimation("WalkLeft");
                    break;
            }
        }

        private void PlayerIdleAnimation(bool idle)
        {
            if (idle)
                switch (_player.lastDirection)
                {
                    case 0:
                        _player.SpriteSheet.ActivateAnimation("IdleUp");
                        break;
                    case 1:
                        _player.SpriteSheet.ActivateAnimation("IdleRight");
                        break;
                    case 2:
                        _player.SpriteSheet.ActivateAnimation("IdleDown");
                        break;
                    case 3:
                        _player.SpriteSheet.ActivateAnimation("IdleLeft");
                        break;
                }
        }
    }
}