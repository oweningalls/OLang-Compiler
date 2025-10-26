using OLangCompiler.Parser.ParseTree.Expression;
using OLangCompiler.Parser.ParseTree.Scope;

namespace OLangCompiler.Parser.ParseTree.Stmt;

public class WhileStatement(BaseExpressionNode condition, ScopeNode scope) : IStatementNode
{
    public BaseExpressionNode Condition = condition;
    public ScopeNode Scope = scope;
}