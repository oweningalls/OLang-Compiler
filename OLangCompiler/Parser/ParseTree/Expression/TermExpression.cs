using OLangCompiler.Parser.ParseTree.Term;

namespace OLangCompiler.Parser.ParseTree.Expression;

public class TermExpression(BaseTermNode term) : BaseExpressionNode
{
    public BaseTermNode Term = term;
}