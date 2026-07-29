using OLangAst.Expressions;

namespace OLangAst.Statements;

public class WhileLoop : IStatement
{
    public IExpression Predicate;
    public Scope Body;
}