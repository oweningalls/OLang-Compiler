using OLangAst.Expressions;

namespace OLangAst.Statements;

public class VariableAssignment : IStatement
{
    public string Identifier;
    public IExpression Value;
}