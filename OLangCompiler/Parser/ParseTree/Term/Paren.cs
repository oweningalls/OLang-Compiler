using OLangCompiler.Parser.ParseTree.Expression;

namespace OLangCompiler.Parser.ParseTree.Term;

public class Paren(IExpression expression) : ITerm
{
    public IExpression Expression = expression;
}