using Lexing;
using OLangAst.TypeSystem;

namespace OLangAst.Expressions;

public class Instantiation(string className) : IExpression
{
    public string ClassName = className;
    public SourceSpan Span { get; set; }
    public DefinedType? Type { get; set; }
}