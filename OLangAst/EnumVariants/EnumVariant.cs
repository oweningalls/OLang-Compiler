using Lexing;
using OLangAst.Miscellaneous;

namespace OLangAst.EnumVariants;

public class EnumVariant(string name, List<IVariableType> values) : IAstNode
{
    public string Name = name;
    public List<IVariableType> Values = values;
    public SourceSpan Span { get; set; }
}