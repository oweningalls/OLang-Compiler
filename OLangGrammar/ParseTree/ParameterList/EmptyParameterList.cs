using Lexing;

namespace OLangGrammar.ParseTree.ParameterList;

public class EmptyParameterList : IParameterListNode
{
    public SourceSpan Span { get; set; }
}