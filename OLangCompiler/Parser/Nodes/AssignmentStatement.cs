using OLangCompiler.Tokens;

namespace OLangCompiler.Parser.Nodes;

public class AssignmentStatement(IdentifierToken identifier, BaseExpressionNode expression) : IStatementNode
{
    public IdentifierToken Identifier = identifier;
    public BaseExpressionNode Expression = expression;
}