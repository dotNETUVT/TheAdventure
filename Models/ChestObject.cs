using Silk.NET.Maths;
using TheAdventure;

namespace kbradu
{
    /// <summary>
    /// Side question: How  the engine to takes care of alpha value of the textures?
    /// </summary>
    [Serializable]
    public class ChestObject : RenderableGameObject
    {
        private static LinkedList<ChestObject> _chests = new LinkedList<ChestObject>();
        
        public bool IsOpen { get; private set; } = false;
        public MaterialType Type { get; private set; }
        public Coin[] _coins { get; private set; }
        public PlayerObject _player { get; private set; }

        public int xPos { get; private set; }   
        public int yPos { get; private set; }   

        public ChestObject(int id, int x, int y, int coinsNum, MaterialType type, PlayerObject player) : 
            base(Path.Combine("Assets", type == MaterialType.Silver? "silver_chest.png" : "gold_chest.png"), id)
        {
            _chests.AddLast(this);
            _coins = new Coin[coinsNum];
            for (int i = 0; i < coinsNum; i++)
            {
                _coins[i] = new Coin(type);
            }
            Type = type;

            _player = player;

            TextureDestination = new Rectangle<int>(x, y, 18, 13);

            this.xPos = x;
            this.yPos = y;
        }

        public override void Update()
        {
            // Open on collision, give to the player the coins
            float distance = MathF.Sqrt(MathF.Pow(xPos - _player.X, 2) + MathF.Pow(yPos - _player.Y, 2)); 

            if(!IsOpen && distance < 40 && _player.IsInteracting)
            {
                IsOpen = true;
                _player.pocket.AddRange(_coins);
                 _coins = null;
                
                var fileName = Path.Combine("Assets", Type == MaterialType.Silver ? "silver_chest_open.png" : "gold_chest_open.png");
                TextureId = GameRenderer.LoadTexture(fileName, out var textureData);
                TextureInformation = textureData;
                TextureSource = new Silk.NET.Maths.Rectangle<int>(0, 0, textureData.Width, textureData.Height);
                
            }
         

  
          

        }
    }
}
