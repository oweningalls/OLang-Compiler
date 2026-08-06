using Lexing;

namespace OLangGrammar.ParseTree.AssignmentOperator;

public class Equals: IAssignmentOperator
{
    public SourceSpan Span { get; set; }
}