using OLangCompiler.Parser.ParseTree;

namespace OLangCompiler.Parser.BottomUpParser;

public class GrammarHelper(IGrammar grammar)
{
    public List<BaseGrammarRule> GetProductionsFor(Type type)
    {
        if (!typeof(INode).IsAssignableFrom(type))
        {
            return [];
        }
        var interfaceType = type.GetInterfaces().Except(type.GetInterfaces().SelectMany(i => i.GetInterfaces())).Single();
        if (type == typeof(INode))
        {
            throw new Exception($"Nonterminal to get productions for must directly implement an interface other than INode, was {type}");
        }
        return grammar.GetRules().Where(x => x.GetLhsType() == interfaceType).ToList();
    }
}