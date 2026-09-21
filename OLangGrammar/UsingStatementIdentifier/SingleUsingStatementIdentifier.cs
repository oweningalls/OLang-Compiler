using Lexing;
using OLangTokens.Tokens;

namespace OLangGrammar.UsingStatementIdentifier;

public class SingleUsingStatementIdentifier(IdentifierToken identifier) : IUsingStatementIdentifier
{
    public IdentifierToken Identifier = identifier;
    public SourceSpan Span { get; set; }
}