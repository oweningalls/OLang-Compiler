using OLangCompiler.Tokens;
using OLangCompiler.TypeChecking.Types;

namespace OLangCompiler.Parser.Nodes;

public class DeclarationStatement(IdentifierToken identifier, BaseExpressionNode expression, ExpressionType? expressionType = null) : IStatementNode
{
    public IdentifierToken Identifier = identifier;
    public BaseExpressionNode Expression = expression;
    public ExpressionType? ExpressionType = expressionType;
}