namespace OLangAst.TypeSystem;

public static class PrimitiveTypes
{
    // TODO: only use C# types if targeting CIL
    public static readonly DefinedType BoolType = DefinedType.FromCsType(typeof(bool));
    public static readonly DefinedType IntType = DefinedType.FromCsType(typeof(int));
    public static readonly DefinedType FloatType = DefinedType.FromCsType(typeof(float));
    public static readonly DefinedType StringType = DefinedType.FromCsType(typeof(string));
    public static readonly DefinedType VoidType = DefinedType.FromCsType(typeof(void));
}