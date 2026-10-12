namespace OLangAst.TypeSystem;

public class DefinedArray(IType innerType) : IType
{
    public readonly IType InnerType = innerType;
    public override bool Equals(object? obj)
    {
        return obj is DefinedArray definedArray && definedArray.InnerType.Equals(InnerType);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(InnerType.GetHashCode(), "[]");
    }

    public override string ToString()
    {
        return $"[{InnerType}]";
    }
}