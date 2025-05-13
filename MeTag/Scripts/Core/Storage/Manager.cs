// Основной менеджер истории
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class Manager<T> where T : class
{
    private readonly IStorage _storage;
    private readonly int _maxHistoryEntries;
    private List<T> _cache;
    private readonly SemaphoreSlim _cacheLock = new SemaphoreSlim(1, 1);

    public event EventHandler HistoryChanged;

    public Manager(IStorage storage, int maxHistoryEntries = 200)
    {
        _storage = storage;
        _maxHistoryEntries = maxHistoryEntries;
    }

    public async Task InitializeAsync()
    {
        await _cacheLock.WaitAsync();
        try
        {
            _cache = await _storage.LoadAsync<T>();
            TruncateHistory();
        }
        finally
        {
            _cacheLock.Release();
        }
    }

    public virtual async Task AddEntryAsync(T entry)
    {
        await _cacheLock.WaitAsync();
        try
        {
            _cache.Insert(0, entry);
            TruncateHistory();
            await _storage.SaveAsync(_cache);
            HistoryChanged?.Invoke(this, EventArgs.Empty);
        }
        finally
        {
            _cacheLock.Release();
        }
    }

    public async Task<IReadOnlyList<T>> GetHistoryAsync(
        Func<T, bool> filter = null,
        int? limit = null)
    {
        await _cacheLock.WaitAsync();
        try
        {
            var query = _cache.AsEnumerable();
            if (filter != null) query = query.Where(filter);
            if (limit.HasValue) query = query.Take(limit.Value);
            return query.ToList();
        }
        finally
        {
            _cacheLock.Release();
        }
    }

    private void TruncateHistory()
    {
        if (_cache.Count > _maxHistoryEntries)
        {
            _cache = _cache.GetRange(0, _maxHistoryEntries);
        }
    }
}