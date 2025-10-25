using OLangCompiler.Tokens;

namespace OLangCompiler.Parser.Nodes;

public class AssignmentStatement(IdentifierToken identifier, IAssignmentOperatorNode operatorNode, BaseExpressionNode expression) : IStatementNode
{
    public IdentifierToken Identifier = identifier;
    public IAssignmentOperatorNode Operator = operatorNode;
    public BaseExpressionNode Expression = expression;
}