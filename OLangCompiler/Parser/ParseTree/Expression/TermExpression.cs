using OLangCompiler.Parser.ParseTree.Term;

namespace OLangCompiler.Parser.ParseTree.Expression;

public class TermExpression(BaseTermNode term) : BaseExpression
{
    public BaseTermNode Term = term;
}