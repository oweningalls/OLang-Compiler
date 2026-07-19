using OLangCompiler.Parser.ParseTree;
using OLangCompiler.Tokens;

namespace OLangCompiler.Parser.BottomUpParser;

public class GrammarHelper(IGrammar grammar)
{
    public List<BaseGrammarRule> GetProductionsFor(Type symbol)
    {
        if (!typeof(INode).IsAssignableFrom(symbol))
        {
            return [];
        }

        if (!symbol.IsInterface)
        {
            throw new ArgumentException($"Can't get productions for {symbol}, only for interfaces");
        }
        return grammar.GetRules().Where(x => x.GetLhsType() == symbol).ToList();
    }

    // returns a list of BaseNode types that can be the first terminal of symbol
    public HashSet<Type> First(Type symbol)
    {
        if (typeof(BaseToken).IsAssignableFrom(symbol))
        {
            return [symbol];
        }

        if (!typeof(INode).IsAssignableFrom(symbol))
        {
            throw new ArgumentException($"Symbol {symbol.Name} isn't a {nameof(BaseToken)} or a {nameof(INode)}");
        }

        return GetProductionsFor(symbol).SelectMany(x => First(x.GetRhsTypes()[0])).ToHashSet();
    }
}