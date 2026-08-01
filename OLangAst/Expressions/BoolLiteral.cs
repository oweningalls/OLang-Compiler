namespace OLangAst.Expressions;

public class BoolLiteral(bool value) : IExpression
{
    public bool Value = value;
}