using Lexing;

namespace OLangGrammar.ParseTree.AssignmentOperator;

public class DivideEquals : IAssignmentOperator
{
    public SourceSpan Span { get; set; }
}