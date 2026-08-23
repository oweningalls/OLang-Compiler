using OLangAst.Miscellaneous;

namespace OLangAst.Expressions;

public class Multiply(IExpression lhs, IExpression rhs) : BaseBinaryExpression(lhs, rhs)
{
    public override IVariableType? Type { get; set; }
}
