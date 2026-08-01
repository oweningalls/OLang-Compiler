namespace OLangAst.Expressions;

public class Not(IExpression value) : IExpression
{
    public IExpression Value = value;
}