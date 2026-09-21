using Lexing;
using OLangTokens.Tokens;

namespace OLangGrammar.UsingStatementIdentifier;

public class ContinuedUsingStatementIdentifier(IdentifierToken identifier, IUsingStatementIdentifier continuedIdentifier) : IUsingStatementIdentifier
{
    public IdentifierToken Identifier = identifier;
    public IUsingStatementIdentifier ContinuedIdentifier = continuedIdentifier;
    public SourceSpan Span { get; set; }
}