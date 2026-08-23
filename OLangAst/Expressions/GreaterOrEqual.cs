using OLangAst.Miscellaneous;

namespace OLangAst.Expressions;

public class GreaterOrEqual(IExpression lhs, IExpression rhs) : BaseBinaryExpression(lhs, rhs)
{
    public override IVariableType? Type { get; set; } = new PrimitiveVariableType(PrimitiveVariableTypeEnum.Bool);
}
