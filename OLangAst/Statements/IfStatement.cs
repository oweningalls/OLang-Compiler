using OLangAst.Expressions;

namespace OLangAst.Statements;

public class IfStatement : IStatement
{
    public IExpression Predicate;
    public Scope Body;
    public ElseBlock? Else;
}