using Lexing;

namespace Parser.Parser.BottomUpParser;

public class GrammarHelper
{
    private readonly IGrammar _grammar;
    private readonly MethodCacher _methodCacher;
    
    public GrammarHelper(IGrammar grammar)
    {
        _grammar = grammar;
        _methodCacher = new MethodCacher();
    }

    public List<BaseGrammarRule> GetProductionsFor(Type symbol)
    {
        return _methodCacher.RunMethodWithCaching(GetProductionsForImpl, symbol);
    }
    private List<BaseGrammarRule> GetProductionsForImpl(Type symbol)
    {
        if (!symbol.IsAssignableTo(typeof(INode)))
        {
            return [];
        }

        if (!symbol.IsInterface)
        {
            throw new ArgumentException($"Can't get productions for {symbol}, only for interfaces");
        }
        
        return _grammar.GetRules().Where(x => x.GetLhsType() == symbol).ToList();
    }
    

    // returns a list of BaseNode types that can be the first terminal of symbol
    public HashSet<Type> First(Type symbol)
    {
        return _methodCacher.RunMethodWithCaching(FirstImpl, symbol);
    }
    
    private HashSet<Type> FirstImpl(Type symbol)
    {
        _seenTypesForFirst = [];
        return FirstWithSeenTypeChecking(symbol);
    }

    private HashSet<Type> FirstWithSeenTypeChecking(Type symbol)
    {
        if (symbol.IsAssignableTo(typeof(BaseToken)))
        {
            return [symbol];
        }

        if (!symbol.IsAssignableTo(typeof(INode)))
        {
            throw new ArgumentException($"Symbol {symbol.Name} isn't a {nameof(BaseToken)} or a {nameof(INode)}");
        }
        
        if (_seenTypesForFirst.Contains(symbol))
        {
            return [];
        }

        _seenTypesForFirst.Add(symbol);

        return GetProductionsFor(symbol).SelectMany(x =>
        {
            var rhsTypes = x.GetRhsTypes();
            if (rhsTypes.Count == 0)
            {
                return [typeof(Epsilon)];
            }
            
            var first = HandleEpsilonsInFirst(rhsTypes);

            return first;
        }).ToHashSet();
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
