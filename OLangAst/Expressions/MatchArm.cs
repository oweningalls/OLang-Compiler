using Lexing;

namespace OLangAst.Expressions;

public class MatchArm(string enumName, string variantName, List<string> destructureVariables, IExpression value) : IAstNode
{
    public string EnumName = enumName;
    public string VariantName = variantName;
    public List<string> DestructureVariables = destructureVariables;
    public IExpression Value = value;
    public SourceSpan Span { get; set; }
}