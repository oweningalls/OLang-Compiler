using Lexing;
using OLangGrammar.ParseTree.Type;

namespace OLangGrammar.ParseTree.EnumValueList;

public class ContinuedEnumValueList(IType type, IEnumValueList enumValueList) : IEnumValueList
{
    public IType Type = type;
    public IEnumValueList EnumValueList = enumValueList;
    public SourceSpan Span { get; set; }
}