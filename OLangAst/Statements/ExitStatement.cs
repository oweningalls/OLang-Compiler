using OLangAst.Expressions;

namespace OLangAst.Statements;

public class ExitStatement(IExpression expression) : IStatement
{
    public IExpression Expression = expression;
}