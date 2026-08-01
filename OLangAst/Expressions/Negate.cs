namespace OLangAst.Expressions;

public class Negate(IExpression value) : IExpression
{
    public IExpression Value = value;
}