using OLangCompiler.Tokens;

namespace OLangCompiler.Parser.Nodes;

public class AssignmentStatement(IdentifierToken identifier, IExpressionNode expression) : IStatementNode
{
    public IdentifierToken Identifier = identifier;
    public IExpressionNode Expression = expression;
}