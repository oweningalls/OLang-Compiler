using Lexing;

namespace OLangGrammar.ParseTree.AssignmentOperator;

public class TimesEquals : IAssignmentOperator
{
    public SourceSpan Span { get; set; }
}