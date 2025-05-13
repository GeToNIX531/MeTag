using System;
using System.IO;
using System.Threading.Tasks;

public class ValidatingManager<T> : Manager<T> where T : class
{
    private readonly Func<T, bool> _validationRule;

    public ValidatingManager(
        IStorage storage,
        Func<T, bool> validationRule,
        int maxEntries = 200) : base(storage, maxEntries)
    {
        _validationRule = validationRule;
    }

    public override async Task AddEntryAsync(T entry)
    {
        if (!_validationRule(entry)) throw new InvalidDataException();
        await base.AddEntryAsync(entry);
    }
}