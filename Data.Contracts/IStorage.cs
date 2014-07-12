namespace Data.Contracts
{
    public interface IStorage
    {
        object Add(string data);
        string[] Get(object filteringObject);
    }
}