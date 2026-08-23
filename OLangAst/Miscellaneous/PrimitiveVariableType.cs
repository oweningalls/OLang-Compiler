using Lexing;

namespace OLangAst.Miscellaneous;

public class PrimitiveVariableType(PrimitiveVariableTypeEnum type) : IVariableType
{
    public PrimitiveVariableTypeEnum Type = type;
    public SourceSpan Span { get; set; }

    public override bool Equals(object? obj)
    {
        if (obj is not PrimitiveVariableType primitiveVariableType)
        {
            return false;
        }

        return primitiveVariableType.Type == Type;
    }
}

public enum PrimitiveVariableTypeEnum
{
    Int,
    Bool,
    Float
}