using OLangCompiler.Parser.ParseTree;

namespace OLangCompiler.Parser.BottomUpParser;

public abstract class BaseGrammarRule
{
    public abstract Type GetLhsType();
    public abstract IReadOnlyList<Type> GetRhsTypes();
    public abstract INode ReduceRule(List<IGrammarElement> rhs);

    public override string ToString()
    {
        return $"{GetLhsType().Name} -> {string.Join(" ", GetRhsTypes().Select(x => x.Name).ToList())}";
    }
}