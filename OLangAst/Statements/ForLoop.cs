using OLangAst.Expressions;

namespace OLangAst.Statements;

public class ForLoop : IStatement
{
    public string Identifier;
    public IExpression RangeStart;
    public IExpression RangeEnd;
    public Scope Body;
}