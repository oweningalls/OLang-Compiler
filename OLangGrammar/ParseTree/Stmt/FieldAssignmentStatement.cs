using Lexing;
using OLangGrammar.ParseTree.AssignmentOperator;
using OLangGrammar.ParseTree.Expression;
using OLangGrammar.ParseTree.Term;
using OLangTokens.Tokens;

namespace OLangGrammar.ParseTree.Stmt;

public class FieldAssignmentStatement(ITerm target, IdentifierToken identifier, IAssignmentOperator assignmentOperator, IExpression value) : IStatement
{
    public ITerm Target = target;
    public IdentifierToken Identifier = identifier;
    public IAssignmentOperator AssignmentOperator = assignmentOperator;
    public IExpression Value = value;
    public SourceSpan Span { get; set; }
}