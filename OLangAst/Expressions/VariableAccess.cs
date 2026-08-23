using Lexing;
using OLangAst.Miscellaneous;

namespace OLangAst.Expressions;

public class VariableAccess(string identifier) : IExpression
{
    public string Identifier = identifier;
    public SourceSpan Span { get; set; }
    public IVariableType? Type { get; set; }
}