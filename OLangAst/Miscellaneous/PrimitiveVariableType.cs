namespace OLangAst.Miscellaneous;

public class PrimitiveVariableType(PrimitiveVariableTypeEnum type) : IVariableType
{
    public PrimitiveVariableTypeEnum Type = type;
}

public enum PrimitiveVariableTypeEnum
{
    Int,
    Bool,
    Float
}