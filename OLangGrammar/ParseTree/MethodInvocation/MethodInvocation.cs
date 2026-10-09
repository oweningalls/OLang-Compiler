using Lexing;
using OLangTokens.Tokens;

namespace OLangGrammar.ParseTree.MethodInvocation;

public class MethodInvocation(IdentifierToken type, IdentifierToken identifier) : IMethodInvocation
{
    public IdentifierToken Type = type;
    public IdentifierToken Identifier = identifier;
    public SourceSpan Span { get; set; }
}