using Lexing;
using OLangGrammar.ParseTree.Type;

namespace OLangGrammar.ParseTree.TypeArgumentList;

public class SingleTypeArgumentList(IType type) : ITypeArgumentList
{
    public IType Type = type;
    public SourceSpan Span { get; set; }
}