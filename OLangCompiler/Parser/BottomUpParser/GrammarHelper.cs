using OLangCompiler.Parser.ParseTree;

namespace OLangCompiler.Parser.BottomUpParser;

public class GrammarHelper(IGrammar grammar)
{
    private readonly IGrammar _grammar = grammar;

    public List<BaseGrammarRule> GetProductionsFor(Type type)
    {
        if (type.IsAssignableFrom(typeof(INode)))
        {
            throw new Exception($"Nonterminal to get productions for must extend INode, was {type}");
        }
        return _grammar.GetRules().Where(x => x.GetLhsType() == type).ToList();
    }
}