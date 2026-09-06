using Lexing;
using OLangGrammar.ParseTree.Term;
using OLangTokens.Tokens;

namespace OLangGrammar.ParseTree.MethodInvocation;

public class MethodInvocation(ITerm term, IdentifierToken identifier) : IMethodInvocation
{
    public ITerm Term = term;
    public IdentifierToken Identifier = identifier;
    public SourceSpan Span { get; set; }
}