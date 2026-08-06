using Lexing;

namespace OLangGrammar.ParseTree.ArgumentList;

public class EmptyArgumentList : IArgumentList
{
    public SourceSpan Span { get; set; }
}