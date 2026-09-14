using OLangAst.TypeSystem;

namespace OLangAst.Expressions;

public class Multiply(IExpression lhs, IExpression rhs) : BaseBinaryExpression(lhs, rhs)
{
    public override DefinedType? Type { get; set; }
}
