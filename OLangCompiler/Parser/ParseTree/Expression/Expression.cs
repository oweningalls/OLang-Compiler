using OLangCompiler.Parser.ParseTree.AndBinaryExpression;

namespace OLangCompiler.Parser.ParseTree.Expression;

public class Expression(IExpression lhs, IAndExpression rhs) : IExpression
{
    public IExpression Lhs = lhs;
    public IAndExpression Rhs = rhs;
}