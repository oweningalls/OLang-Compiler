namespace OLangAst.Expressions;

public class VariableAccess(string identifier) : IExpression
{
    public string Identifier = identifier;
}