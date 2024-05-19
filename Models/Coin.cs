namespace TheAdventure.Models;

public class Coin : TemporaryGameObject
{
    public enum CoinState
    {
        None = 0,
        Spinning,
        Fading,
        Collected,
        Faded
    }

    public CoinState State { get; private set; }

    public Coin((int X, int Y) position, SpriteSheet spriteSheet) : base(spriteSheet, 4, position)
    {
        State = CoinState.Spinning;
        SpriteSheet.ActivateAnimation("Spinning");
    }

    public void SetState(bool playerCollision)
    {
        if (playerCollision)
        {
            State = CoinState.Collected;
            
            return;
        }
        if ((DateTimeOffset.Now - _spawnTime).TotalSeconds >= Ttl-0.3)
        {
            State = CoinState.Fading;
            SpriteSheet.ActivateAnimation("Fading");
        }
        if (IsExpired)
        {
            State = CoinState.Faded;
        }
    }
}