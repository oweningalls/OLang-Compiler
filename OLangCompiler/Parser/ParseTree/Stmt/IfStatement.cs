using OLangCompiler.Parser.ParseTree.ElseBlock;
using OLangCompiler.Parser.ParseTree.Expression;
using OLangCompiler.Parser.ParseTree.Scope;

namespace OLangCompiler.Parser.ParseTree.Stmt;

public class IfStatement(BaseExpressionNode condition, ScopeNode scope, IElseNode elseBlock) : IStatementNode
{
    public BaseExpressionNode Condition = condition;
    public ScopeNode Scope = scope;
    public IElseNode ElseBlock = elseBlock;
}