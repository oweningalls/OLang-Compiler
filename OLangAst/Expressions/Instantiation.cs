using Lexing;
using OLangAst.TypeSystem;

namespace OLangAst.Expressions;

public class Instantiation(string className, Dictionary<string, IExpression> initializations) : IExpression
{
    public string ClassName = className;
    public Dictionary<string, IExpression> FieldInitializations = initializations;
    public SourceSpan Span { get; set; }
    public ConcreteType? Type { get; set; }
}