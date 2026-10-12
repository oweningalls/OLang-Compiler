using Lexing;
using OLangAst.Miscellaneous;
using OLangAst.TypeSystem;

namespace OLangAst.Expressions;

public class EnumInstantiation(string enumName, List<IVariableType> typeArguments, string variantName, List<IExpression> arguments) : IExpression
{
    public string EnumName = enumName;
    public List<IVariableType> TypeArguments = typeArguments;
    public string VariantName = variantName;
    public List<IExpression> Arguments = arguments;
    public SourceSpan Span { get; set; }
    public IType? Type { get; set; }
    public List<IType>? ResolvedTypeArguments;
}