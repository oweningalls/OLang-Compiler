using Lexing;

namespace OLangGrammar.ParseTree.Type;

public class ArrayType(IType innerType) : IType
{
    public IType InnerType = innerType;
    public SourceSpan Span { get; set; }
}