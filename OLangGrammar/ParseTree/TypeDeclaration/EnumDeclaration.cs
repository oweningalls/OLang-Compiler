using Lexing;
using OLangGrammar.ParseTree.EnumVariantList;
using OLangTokens.Tokens;

namespace OLangGrammar.ParseTree.TypeDeclaration;

public class EnumDeclaration(IdentifierToken identifierToken, IEnumVariantList enumVariantList) : ITypeDeclaration
{
    public IdentifierToken IdentifierToken = identifierToken;
    public IEnumVariantList EnumVariantList = enumVariantList;
    public SourceSpan Span { get; set; }
}