namespace OLangAst.Expressions;

public class IntLiteral(int value) : IExpression
{
    public int Value = value;
}
