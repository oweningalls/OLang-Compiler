using Lexing;
using OLangGrammar.ParseTree.AssignmentOperator;
using OLangGrammar.ParseTree.Expression;
using OLangGrammar.ParseTree.Term;

namespace OLangGrammar.ParseTree.Stmt;

public class ArrayAssignmentStatement(ITerm array, IExpression index, IAssignmentOperator assignmentOperator, IExpression value) : IStatement
{
    public ITerm Array = array;
    public IExpression Index = index;
    public IAssignmentOperator AssignmentOperator = assignmentOperator;
    public IExpression Value = value;
    public SourceSpan Span { get; set; }
}