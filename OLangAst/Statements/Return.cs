using OLangAst.Expressions;

namespace OLangAst.Statements;

public class Return(IExpression? value) : IStatement
{
    public IExpression? Value = value;
}