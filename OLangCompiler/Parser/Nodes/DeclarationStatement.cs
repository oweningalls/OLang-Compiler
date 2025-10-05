using OLangCompiler.Tokens;
using OLangCompiler.TypeChecking.Types;

namespace OLangCompiler.Parser.Nodes;

public class DeclarationStatement(IdentifierToken identifier, IExpressionNode expression, ExpressionType? expressionType = null) : IStatementNode
{
    public IdentifierToken Identifier = identifier;
    public IExpressionNode Expression = expression;
    public ExpressionType? ExpressionType = expressionType;
}