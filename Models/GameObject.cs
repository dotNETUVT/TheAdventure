public class GameObject
{
    public int Id{get; private set;}
    public GameObject(int id){
        Id = id;
    }    

    public virtual void Update() { }
}