using Lexing;

namespace OLangGrammar.ParseTree.AssignmentOperator;

public class PlusEquals : IAssignmentOperator
{
    public SourceSpan Span { get; set; }
}