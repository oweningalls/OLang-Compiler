using Lexing;

namespace OLangGrammar.ParseTree.AssignmentOperator;

public class MinusEquals : IAssignmentOperator
{
    public SourceSpan Span { get; set; }
}