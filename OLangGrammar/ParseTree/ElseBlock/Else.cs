using Lexing;
using OLangGrammar.ParseTree.Scope;

namespace OLangGrammar.ParseTree.ElseBlock;

public class Else(IScopeNode scope) : IElse
{
    public IScopeNode Scope = scope;
    public SourceSpan Span { get; set; }
}