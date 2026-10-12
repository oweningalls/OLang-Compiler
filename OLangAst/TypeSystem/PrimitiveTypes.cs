namespace OLangAst.TypeSystem;

public static class PrimitiveTypes
{
    // TODO: only use C# types if targeting CIL
    public static readonly DefinedType BoolType = ConcreteType.FromCsType(typeof(bool));
    public static readonly DefinedType IntType = ConcreteType.FromCsType(typeof(int));
    public static readonly DefinedType FloatType = ConcreteType.FromCsType(typeof(float));
    public static readonly DefinedType StringType = ConcreteType.FromCsType(typeof(string));
    public static readonly DefinedType VoidType = ConcreteType.FromCsType(typeof(void));
}