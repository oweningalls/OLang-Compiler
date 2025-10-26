using OLangCompiler.Parser.ParseTree;

namespace OLangCompiler.Parser.BottomUpParser;

public interface IGrammarRule
{
    public Type GetLhsType();
    public List<Type> GetRhsTypes();

    public INode ReduceRule(List<IGrammarElement> rhs);
}