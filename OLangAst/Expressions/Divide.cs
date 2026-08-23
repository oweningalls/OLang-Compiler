using OLangAst.Miscellaneous;

namespace OLangAst.Expressions;

public class Divide(IExpression lhs, IExpression rhs) : BaseBinaryExpression(lhs, rhs)
{
    public override IVariableType? Type { get; set; }
}
