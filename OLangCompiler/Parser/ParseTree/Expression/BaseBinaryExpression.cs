namespace OLangCompiler.Parser.ParseTree.Expression;

public abstract class BaseBinaryExpression(BaseExpression lhs, BaseExpression rhs) : BaseExpression
{
    public BaseExpression Lhs = lhs;
    public BaseExpression Rhs = rhs;
}