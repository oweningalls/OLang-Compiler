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

    protected bool Equals(PrimitiveVariableType other)
    {
        return Type == other.Type;
    }

    public override int GetHashCode()
    {
        return (int)Type;
    }

    public override string ToString()
    {
        return Type.ToString();
    }
}

public enum PrimitiveVariableTypeEnum
{
    Int,
    Bool,
    Float,
    String
}