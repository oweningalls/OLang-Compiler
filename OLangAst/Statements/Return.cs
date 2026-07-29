using OLangAst.Expressions;

namespace OLangAst.Statements;

public class Return : IStatement
{
    public IExpression? Value;
}