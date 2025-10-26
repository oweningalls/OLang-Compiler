using OLangCompiler.Parser.ParseTree.AddBinaryExpression;

namespace OLangCompiler.Parser.ParseTree.GreaterBinaryExpression;

public class Less(IGreaterExpression lhs, IAddExpression rhs) : IGreaterExpression
{
    public IGreaterExpression Lhs = lhs;
    public IAddExpression Rhs = rhs;
}
