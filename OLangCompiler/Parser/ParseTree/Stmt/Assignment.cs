using OLangCompiler.Parser.ParseTree.AssignmentOperator;
using OLangCompiler.Parser.ParseTree.Expression;
using OLangTokens.Tokens;

namespace OLangCompiler.Parser.ParseTree.Stmt;

public class Assignment(IdentifierToken identifier, IAssignmentOperator operatorNode, IExpression expression) : IStatement
{
    public IdentifierToken Identifier = identifier;
    public IAssignmentOperator Operator = operatorNode;
    public IExpression Expression = expression;
}