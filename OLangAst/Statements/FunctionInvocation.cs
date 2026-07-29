using OLangAst.Expressions;

namespace OLangAst.Statements;

public class FunctionInvocation : IStatement, IExpression
{
    public string Identifier;
    public List<IExpression> Arguments;
}