using Lexing;

namespace OLangAst.Miscellaneous;

public class ArrayType(IVariableType innerType) : IVariableType
{
    public string Name { get; } = innerType.Name;
    public IVariableType InnerType { get; set; } = innerType;
    public SourceSpan Span { get; set; }
}