namespace Questalia;

public enum questType
{
    Active = 0,
    Inactive = 2,
    Done = 1
}
public class Quest
{
    private string _name;
    public int _id { get; private set; }
    private questType _type;
    
    public Quest(string name, int id, questType type)
        : base()
    {
        _name = name;
        _id = id;
        _type = type;
    }
}