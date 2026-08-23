using OLangAst.Miscellaneous;

namespace OLangAst.Expressions;

public class And(IExpression lhs, IExpression rhs) : BaseBinaryExpression(lhs, rhs)
{
    public override IVariableType? Type { get; set; } = new PrimitiveVariableType(PrimitiveVariableTypeEnum.Bool);
}