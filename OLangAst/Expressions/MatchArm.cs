using Lexing;
using OLangAst.Miscellaneous;

namespace OLangAst.Expressions;

public class MatchArm(string enumName, List<IVariableType> typeArguments, string variantName, List<string> destructureVariables, IExpression value) : IAstNode
{
    public string EnumName = enumName;
    public List<IVariableType> TypeArguments = typeArguments;
    public string VariantName = variantName;
    public List<string> DestructureVariables = destructureVariables;
    public IExpression Value = value;
    public SourceSpan Span { get; set; }
}