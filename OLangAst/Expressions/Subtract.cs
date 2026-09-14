using OLangAst.TypeSystem;

namespace OLangAst.Expressions;

public class Subtract(IExpression lhs, IExpression rhs) : BaseBinaryExpression(lhs, rhs)
{
    public override DefinedType? Type { get; set; }
}
