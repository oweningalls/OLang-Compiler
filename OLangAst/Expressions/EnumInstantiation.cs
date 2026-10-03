using Lexing;
using OLangAst.TypeSystem;

namespace OLangAst.Expressions;

public class EnumInstantiation(string enumName, string variantName) : IExpression
{
    public string EnumName = enumName;
    public string VariantName = variantName;
    public SourceSpan Span { get; set; }
    public DefinedType? Type { get; set; }
}