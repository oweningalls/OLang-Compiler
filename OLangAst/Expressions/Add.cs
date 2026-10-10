using OLangAst.Miscellaneous;
using OLangAst.TypeSystem;

namespace OLangAst.Expressions;

public class Add(IExpression lhs, IExpression rhs) : BaseBinaryExpression(lhs, rhs)
{
    public override ConcreteType? Type { get; set; }
}