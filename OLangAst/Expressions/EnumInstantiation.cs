using Lexing;
using OLangAst.TypeSystem;

namespace OLangAst.Expressions;

public class EnumInstantiation(string enumName, string variantName, List<IExpression> arguments) : IExpression
{
    public string EnumName = enumName;
    public string VariantName = variantName;
    public List<IExpression> Arguments = arguments;
    public SourceSpan Span { get; set; }
    public DefinedType? Type { get; set; }
}