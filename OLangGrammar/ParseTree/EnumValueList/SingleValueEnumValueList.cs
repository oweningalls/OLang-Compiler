using Lexing;
using OLangGrammar.ParseTree.Type;

namespace OLangGrammar.ParseTree.EnumValueList;

public class SingleValueEnumValueList(IType type) : IEnumValueList
{
    public IType Type = type;
    public SourceSpan Span { get; set; }
}