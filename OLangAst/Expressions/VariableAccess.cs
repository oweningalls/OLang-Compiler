using Lexing;
using OLangAst.TypeSystem;

namespace OLangAst.Expressions;

public class VariableAccess(string identifier) : IExpression
{
    public string Identifier = identifier;
    public SourceSpan Span { get; set; }
    public DefinedType? Type { get; set; }
}