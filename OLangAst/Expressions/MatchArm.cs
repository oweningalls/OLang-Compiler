using Lexing;

namespace OLangAst.Expressions;

public class MatchArm(string enumName, string variantName, IExpression value) : IAstNode
{
    public string EnumName = enumName;
    public string VariantName = variantName;
    public IExpression Value = value;
    public SourceSpan Span { get; set; }
}