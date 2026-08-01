using OLangAst.Expressions;

namespace OLangAst.Statements;

public class WhileLoop(IExpression predicate, Scope body) : IStatement
{
    public IExpression Predicate = predicate;
    public Scope Body = body;
}