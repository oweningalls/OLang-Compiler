using Lexing;
using OLangAst.TypeSystem;

namespace OLangAst.Expressions;

public class Instantiation(string className, List<IExpression> arguments) : IExpression
{
    public string ClassName = className;
    public List<IExpression> Arguments = arguments;
    public SourceSpan Span { get; set; }
    public DefinedType? Type { get; set; }
}