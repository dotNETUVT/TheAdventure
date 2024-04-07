using Silk.NET.Maths;

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

        public ChestObject(int id, int x, int y, int coinsNum, MaterialType type) : 
            base(Path.Combine("Assets", type == MaterialType.Silver? "silver_chest.png" : "gold_chest.png"), id)
        {
            _chests.AddLast(this);
            _coins = new Coin[coinsNum];
            for (int i = 0; i < coinsNum; i++)
            {
                _coins[i] = new Coin(type);
            }
            Type = type;

            TextureDestination = new Rectangle<int>(x, y, 18, 13);
        }

        public void Open(out Coin[] coins)
        {
            IsOpen = true;
            coins = _coins;
            this._coins = null;
        }
    }
}
