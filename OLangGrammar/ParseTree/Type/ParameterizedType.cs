using Lexing;
using OLangGrammar.ParseTree.TypeArgumentList;
using OLangTokens.Tokens;

namespace OLangGrammar.ParseTree.Type;

public class ParameterizedType(IdentifierToken type, ITypeArgumentList typeArgumentList) : IType
{
    public IdentifierToken Type = type;
    public ITypeArgumentList TypeArgumentList = typeArgumentList;
    public SourceSpan Span { get; set; }
}