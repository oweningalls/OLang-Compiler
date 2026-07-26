using OLangCompiler.Parser.ParseTree;
using OLangCompiler.Tokens;

namespace OLangCompiler.Parser.BottomUpParser;

public class GrammarHelper(IGrammar grammar)
{
    public List<BaseGrammarRule> GetProductionsFor(Type symbol)
    {
        if (_getProductionsForCache.TryGetValue(symbol, out var productions))
        {
            return productions;
        }
        
        if (!typeof(INode).IsAssignableFrom(symbol))
        {
            return [];
        }

        if (!symbol.IsInterface)
        {
            throw new ArgumentException($"Can't get productions for {symbol}, only for interfaces");
        }
        
        productions = grammar.GetRules().Where(x => x.GetLhsType() == symbol).ToList();
        _getProductionsForCache[symbol] = productions;

        return productions;
    }
    
    private readonly Dictionary<Type, List<BaseGrammarRule>> _getProductionsForCache = [];

    // returns a list of BaseNode types that can be the first terminal of symbol
    public HashSet<Type> First(Type symbol)
    {
        _seenTypesForFirst = [];
        return FirstWithSeenTypeChecking(symbol);
    }

    private readonly Dictionary<Type, HashSet<Type>> _firstCache = [];

    private HashSet<Type> FirstWithSeenTypeChecking(Type symbol)
    {
        if (_firstCache.TryGetValue(symbol, out var set))
        {
            return set;
        }
        
        if (typeof(BaseToken).IsAssignableFrom(symbol))
        {
            return [symbol];
        }

        if (!typeof(INode).IsAssignableFrom(symbol))
        {
            throw new ArgumentException($"Symbol {symbol.Name} isn't a {nameof(BaseToken)} or a {nameof(INode)}");
        }
        
        if (_seenTypesForFirst.Contains(symbol))
        {
            return [];
        }

        _seenTypesForFirst.Add(symbol);

        set = GetProductionsFor(symbol).SelectMany(x =>
        {
            var rhsTypes = x.GetRhsTypes();
            if (rhsTypes.Count == 0)
            {
                return [typeof(Epsilon)];
            }
            
            var first = HandleEpsilonsInFirst(rhsTypes);

            return first;
        }).ToHashSet();
        _firstCache[symbol] = set;

        return set;
    }

    private HashSet<Type> HandleEpsilonsInFirst(IReadOnlyList<Type> rhsTypes)
    {
        var first = FirstWithSeenTypeChecking(rhsTypes[0]);
        var newFirst = first;
        var i = 0;
        while (newFirst.Contains(typeof(Epsilon)) && i < rhsTypes.Count - 1)
        {
            i += 1;
            newFirst = FirstWithSeenTypeChecking(rhsTypes[i]);
            first.UnionWith(newFirst);
        }

        if (!newFirst.Contains(typeof(Epsilon)))
        {
            first.Remove(typeof(Epsilon));
        }

        return first;
    }

    private List<Type> _seenTypesForFirst;
}
class Epsilon : BaseToken;
