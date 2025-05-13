using System.Collections.Generic;
using System.Threading.Tasks;

public interface IStorage
{
    Task SaveAsync<T>(IEnumerable<T> items);
    Task<List<T>> LoadAsync<T>();
    Task ClearAsync();
}