using Lexing;
using OLangGrammar.ParseTree.Expression;
using OLangGrammar.ParseTree.Scope;

namespace OLangGrammar.ParseTree.Stmt;

public class IfWithElse(IExpression condition, IScopeNode scope, IScopeNode elseBlock) : IStatement
{
    public IExpression Condition = condition;
    public IScopeNode Scope = scope;
    public IScopeNode ElseBlock = elseBlock;
    public SourceSpan Span { get; set; }
}