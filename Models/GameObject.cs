namespace TheAdventure.Models;

public class GameObject
{
    private static int _nextId = -1;
    public int Id { get; }

    public GameObject()
    {
        Id = Interlocked.Increment(ref _nextId);
    }
}