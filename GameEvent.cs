namespace TheAdventure;

public enum GameEventTypes
{
    spawnAnimatedGameObject
}

public class GameEvent
{
    public GameEventTypes type;
    // To keep it flexible we use a string to string Dict
    public Dictionary<string, string> _eventSettings { get; private set; }
    
    // Custom counter (like the bombIds)
    public int _counter { get; set; }
  
    
    

    public GameEvent(GameEventTypes type, Dictionary<string, string> settings, int counter)
    {
        this.type = type;
        // Set the settings (this was done with a dict to increase flexibility as different event types may have very different settings). 
        // This is also the same reason to use strings (they are more flexible than int), just remember to convert them in the listener switch case handler
        this._eventSettings = settings;
        // Set the counter (the ID of the event)
        this._counter = counter;
    }
}

