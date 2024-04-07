namespace kbradu
{
    public class Coin
    {
        public MaterialType Type { get; private set; }
        public Coin(MaterialType type = MaterialType.Gold)
        {
            this.Type = type;   
        }
    }

    public enum MaterialType
    {
        Silver,
        Gold
    }
    
}
