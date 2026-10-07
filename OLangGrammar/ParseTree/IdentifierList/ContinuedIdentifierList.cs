using Lexing;
using OLangTokens.Tokens;

namespace OLangGrammar.ParseTree.IdentifierList;

public class ContinuedIdentifierList(IdentifierToken identifier, IIdentifierList identifierList) : IIdentifierList
{
    public IdentifierToken Identifier = identifier;
    public IIdentifierList IdentifierList = identifierList;
    public SourceSpan Span { get; set; }
}
