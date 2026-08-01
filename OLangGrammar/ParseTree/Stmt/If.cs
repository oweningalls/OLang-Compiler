using OLangGrammar.ParseTree.ElseBlock;
using OLangGrammar.ParseTree.Expression;
using OLangGrammar.ParseTree.Scope;

namespace OLangGrammar.ParseTree.Stmt;

public class If(IExpression condition, IScopeNode scope, IElse elseBlock) : IStatement
{
    public IExpression Condition = condition;
    public IScopeNode Scope = scope;
    public IElse ElseBlock = elseBlock;
}