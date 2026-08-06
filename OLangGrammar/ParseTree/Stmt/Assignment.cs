using Lexing;
using OLangGrammar.ParseTree.AssignmentOperator;
using OLangGrammar.ParseTree.Expression;
using OLangTokens.Tokens;

namespace OLangGrammar.ParseTree.Stmt;

public class Assignment(IdentifierToken identifier, IAssignmentOperator operatorNode, IExpression expression) : IStatement
{
    public IdentifierToken Identifier = identifier;
    public IAssignmentOperator Operator = operatorNode;
    public IExpression Expression = expression;
    public SourceSpan Span { get; set; }
}