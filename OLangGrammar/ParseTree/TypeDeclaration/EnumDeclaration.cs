using Lexing;
using OLangTokens.Tokens;

namespace OLangGrammar.ParseTree.TypeDeclaration;

public class EnumDeclaration(IdentifierToken identifierToken) : ITypeDeclaration
{
    public IdentifierToken IdentifierToken = identifierToken;
    public SourceSpan Span { get; set; }
}