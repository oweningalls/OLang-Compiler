using OLangCompiler.Parser.ParseTree.AssignmentOperator;
using OLangCompiler.Parser.ParseTree.Expression;
using OLangCompiler.Tokens;

namespace OLangCompiler.Parser.ParseTree.Stmt;

public class Assignment(IdentifierToken identifier, IAssignmentOperator operatorNode, BaseExpression expression) : IStatement
{
    public IdentifierToken Identifier = identifier;
    public IAssignmentOperator Operator = operatorNode;
    public BaseExpression Expression = expression;
}