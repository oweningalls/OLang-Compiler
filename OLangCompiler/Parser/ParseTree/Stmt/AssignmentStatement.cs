using OLangCompiler.Parser.ParseTree.AssignmentOperator;
using OLangCompiler.Parser.ParseTree.Expression;
using OLangCompiler.Tokens;

namespace OLangCompiler.Parser.ParseTree.Stmt;

public class AssignmentStatement(IdentifierToken identifier, IAssignmentOperatorNode operatorNode, BaseExpressionNode expression) : IStatementNode
{
    public IdentifierToken Identifier = identifier;
    public IAssignmentOperatorNode Operator = operatorNode;
    public BaseExpressionNode Expression = expression;
}