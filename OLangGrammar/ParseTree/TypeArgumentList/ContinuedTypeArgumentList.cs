using Lexing;
using OLangGrammar.ParseTree.Type;

namespace OLangGrammar.ParseTree.TypeArgumentList;

public class ContinuedTypeArgumentList(IType type, ITypeArgumentList typeArgumentList) : ITypeArgumentList
{
    public IType Type = type;
    public ITypeArgumentList TypeArgumentList = typeArgumentList;
    public SourceSpan Span { get; set; }
}