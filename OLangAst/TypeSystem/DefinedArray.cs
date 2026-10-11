namespace OLangAst.TypeSystem;

public class DefinedArray(IType innerType) : IType
{
    public IType InnerType = innerType;
    public override bool Equals(object? obj)
    {
        return obj is DefinedArray definedArray && definedArray.InnerType.Equals(InnerType);
    }
}