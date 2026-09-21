using Lexing;
using OLangGrammar.UsingStatementIdentifier;
using OLangTokens.Tokens;

namespace OLangGrammar.UsingStatement;

public class UsingStatement(IUsingStatementIdentifier usingStatementIdentifier) : IUsingStatement
{
    public IUsingStatementIdentifier UsingStatementIdentifier = usingStatementIdentifier;
    public SourceSpan Span { get; set; }
}