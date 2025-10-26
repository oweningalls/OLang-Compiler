using OLangCompiler.Parser.ParseTree.Expression;

namespace OLangCompiler.Parser.ParseTree.Stmt;

public class ExitStatement(BaseExpressionNode expression) : IStatementNode
{
    public BaseExpressionNode ExpressionNode = expression;
}