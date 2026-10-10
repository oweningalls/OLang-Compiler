namespace OLangAst.TypeSystem;

public static class PrimitiveTypes
{
    // TODO: only use C# types if targeting CIL
    public static readonly ConcreteType BoolType = ConcreteType.FromCsType(typeof(bool));
    public static readonly ConcreteType IntType = ConcreteType.FromCsType(typeof(int));
    public static readonly ConcreteType FloatType = ConcreteType.FromCsType(typeof(float));
    public static readonly ConcreteType StringType = ConcreteType.FromCsType(typeof(string));
    public static readonly ConcreteType VoidType = ConcreteType.FromCsType(typeof(void));
}