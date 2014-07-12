using System.Collections.Generic;

namespace Data.Contracts
{
    public interface IStorage
    {
        object Add(string data);
        IEnumerable<string> Get(object filteringObject);
    }
}