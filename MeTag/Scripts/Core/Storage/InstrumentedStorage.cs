using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

public class InstrumentedStorage : IStorage
{
    private readonly IStorage _innerStorage;
    public long SaveDuration { get; private set; }

    public InstrumentedStorage(IStorage innerStorage)
    {
        _innerStorage = innerStorage;
    }

    public Task ClearAsync() => _innerStorage.ClearAsync();

    public Task<List<T>> LoadAsync<T>() => _innerStorage.LoadAsync<T>();

    public async Task SaveAsync<T>(IEnumerable<T> items)
    {
        var sw = Stopwatch.StartNew();
        await _innerStorage.SaveAsync(items);
        SaveDuration = sw.ElapsedMilliseconds;
        sw.Stop();
        sw = null;
    }
}