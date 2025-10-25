using OLangCompiler.Tokens;

namespace OLangCompiler.Parser.Nodes;

public class AssignmentStatement(IdentifierToken identifier, BaseAssignmentOperatorToken operatorToken, BaseExpressionNode expression) : IStatementNode
{
    public IdentifierToken Identifier = identifier;
    public BaseAssignmentOperatorToken OperatorToken = operatorToken;
    public BaseExpressionNode Expression = expression;
}