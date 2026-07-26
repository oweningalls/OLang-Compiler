namespace OLangCompiler.Parser.BottomUpParser;

public class MethodCacher
{
    private readonly Dictionary<Delegate, Dictionary<object, object>> _methodResultCache = [];

    public TResult RunMethodWithCaching<TInput, TResult>(Func<TInput, TResult> method, TInput input)
    {
        if (!_methodResultCache.ContainsKey(method))
        {
            _methodResultCache[method] = [];
        }

        if (_methodResultCache[method].TryGetValue(input, out var storedValue))
        {
            return (TResult)storedValue;
        }

        var result = method.Invoke(input);
        _methodResultCache[method][input] = result;

        return result;
    }
}
