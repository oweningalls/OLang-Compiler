using OLangCompiler.Tokens;

namespace OLangCompiler.Parser.Nodes.NodeValues;

public class DeclarationStatement(IdentifierToken identifier, IExpressionNode expression) : IStatementNode
{
    public IdentifierToken Identifier = identifier;
    public IExpressionNode Expression = expression;
}