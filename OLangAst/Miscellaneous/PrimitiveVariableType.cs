namespace OLangAst.Miscellaneous;

public class PrimitiveVariableType : IVariableType
{
    public PrimitiveVariableTypeEnum Type;
}

public enum PrimitiveVariableTypeEnum
{
    Int,
    Bool,
    Float
}