using OLangGrammar.ParseTree.Expression;
using OLangGrammar.ParseTree.Scope;

namespace OLangGrammar.ParseTree.Stmt;

public class While(IExpression condition, IScopeNode scope) : IStatement
{
    public IExpression Condition = condition;
    public IScopeNode Scope = scope;
}