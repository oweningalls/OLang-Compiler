using Lexing;

namespace OLangAst.Miscellaneous;

public class PrimitiveVariableType(PrimitiveVariableTypeEnum type) : IVariableType
{
    public PrimitiveVariableTypeEnum Type = type;
    public SourceSpan Span { get; set; }
}

public enum PrimitiveVariableTypeEnum
{
    Int,
    Bool,
    Float
}