using Lexing;
using OLangGrammar.ParseTree.EnumVariantList;
using OLangGrammar.ParseTree.IdentifierList;
using OLangTokens.Tokens;

namespace OLangGrammar.ParseTree.TypeDeclaration;

public class EnumDeclaration(IdentifierToken identifierToken, IEnumVariantList enumVariantList, IIdentifierList? typeParameters) : ITypeDeclaration
{
    public IdentifierToken IdentifierToken = identifierToken;
    public IEnumVariantList EnumVariantList = enumVariantList;
    public IIdentifierList? TypeParameters = typeParameters;
    public SourceSpan Span { get; set; }
}