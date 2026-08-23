using OLangAst.Miscellaneous;

namespace OLangAst.Expressions;

public class Add(IExpression lhs, IExpression rhs) : BaseBinaryExpression(lhs, rhs)
{
    public override IVariableType? Type { get; set; }
}