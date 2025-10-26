using OLangCompiler.Parser.ParseTree.Expression;

namespace OLangCompiler.Parser.ParseTree.Term;

public class ParenTerm(BaseExpressionNode expression) : BaseTermNode
{
    public BaseExpressionNode Expression = expression;
}