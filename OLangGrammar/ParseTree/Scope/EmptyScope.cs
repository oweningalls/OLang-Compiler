using Lexing;

namespace OLangGrammar.ParseTree.Scope;

public class EmptyScope : IScopeNode
{
    public SourceSpan Span { get; set; }
}