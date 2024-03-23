namespace TheAdventure;

public class GameEventListener
{
    private Queue<GameEvent> _eventsQueue = new Queue<GameEvent>();
    // Just to optimize a bit and not listen if there are no events in place.
    private int _numberOfEventsInQueue;
    // The global list of game objects
    public Dictionary<int, GameObject> _gameObjects { get; private set; }
    
    
    public GameEventListener(Dictionary<int, GameObject> gameObjects)
    {
        this._gameObjects = gameObjects;
    }

    public void Listen()
    {
        // Only if there are some events
        if(_numberOfEventsInQueue > 0)
            // There may be more than one event (if I spam-clicked, or if I programmatically dispatched lots of events)
            while(_eventsQueue.Count > 0)
            {
                var gameEvent = _eventsQueue.Peek();
                // Huge switch case to take correct actions depending on what event was dispatched
                switch (gameEvent.type)
                {
                    case GameEventTypes.spawnAnimatedGameObject:
                        AnimatedGameObject gameObject = new AnimatedGameObject(gameEvent._eventSettings["fileName"], int.Parse(gameEvent._eventSettings["durationInSeconds"]), gameEvent._counter, int.Parse(gameEvent._eventSettings["numberOfFrames"]), int.Parse(gameEvent._eventSettings["numberOfColumns"]), int.Parse(gameEvent._eventSettings["numberOfRows"]), int.Parse(gameEvent._eventSettings["x"]), int.Parse(gameEvent._eventSettings["y"]));
                        _gameObjects.Add(gameObject.Id, gameObject);
                        break;
                }
                // Finished with one event from the queue
                _eventsQueue.Dequeue();
                _numberOfEventsInQueue--;
            }
    }

    public void DispatchEvent(GameEventTypes eventType, Dictionary<string, string> eventSettings, int counter)
    {
        // Create a new event
        GameEvent eventToBeDispatched = new GameEvent(eventType, eventSettings, counter);
        // Add it to the queue
        _eventsQueue.Enqueue(eventToBeDispatched);
        // Signal that there is a new event in the house!
        _numberOfEventsInQueue++;
    }
}

