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

        if (!type.IsInterface)
        {
            throw new Exception($"Can't get productions for {type}, only for interfaces");
        }
        return grammar.GetRules().Where(x => x.GetLhsType() == type).ToList();
    }
}