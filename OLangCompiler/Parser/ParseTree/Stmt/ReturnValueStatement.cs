using OLangCompiler.Parser.ParseTree.Expression;

namespace OLangCompiler.Parser.ParseTree.Stmt;

public class ReturnValueStatement(BaseExpressionNode expression) : IStatementNode
{
    public BaseExpressionNode Expression = expression;
}