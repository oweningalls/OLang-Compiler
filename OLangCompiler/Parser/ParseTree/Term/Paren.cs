using OLangCompiler.Parser.ParseTree.Expression;

namespace OLangCompiler.Parser.ParseTree.Term;

public class Paren(BaseExpression expression) : BaseTermNode
{
    public BaseExpression Expression = expression;
}