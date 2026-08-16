namespace OLangCompiler.Utility;

public class ScopeTracker<TKey, TValue> where TKey : notnull
{
    private List<Dictionary<TKey, TValue>> _list = [new()];

    public Dictionary<TKey, TValue> EndScope()
    {
        var ret = _list.Last();
        _list.RemoveAt(_list.Count - 1);
        return ret;
    }

    public void BeginScope()
    {
        _list.Add(new());
    }

    public TValue GetValue(TKey identifier)
    {
        for (var i = _list.Count - 1; i >= 0; i--)
        {
            var dict = _list[i];
            if (dict.TryGetValue(identifier, out var expressionType))
            {
                return expressionType;
            }
        }

        throw new Exception($"Unknown identifier: {identifier}");
    }

    public bool ContainsKey(TKey identifier)
    {
        return _list.Any(x => x.ContainsKey(identifier));
    }

    public void SetValue(TKey key, TValue value)
    {
        _list.Last()[key] = value;
    }
}