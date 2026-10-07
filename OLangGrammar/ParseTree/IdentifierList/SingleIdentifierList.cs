using Lexing;
using OLangTokens.Tokens;

namespace OLangGrammar.ParseTree.IdentifierList;

public class SingleIdentifierList(IdentifierToken identifier) : IIdentifierList
{
    public IdentifierToken Identifier = identifier;
    public SourceSpan Span { get; set; }
}