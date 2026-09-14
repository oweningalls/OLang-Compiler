using Lexing;
using OLangAst.Expressions;
using OLangAst.TypeSystem;

namespace OLangAst.Statements;

public class MethodInvocation(IExpression expression, string identifier, List<IExpression> arguments) : IStatement, IExpression
{
    public IExpression? Expression = expression;
    public string Identifier = identifier;
    public List<IExpression> Arguments = arguments;
    public DefinedType? SourceType;
    public SourceSpan Span { get; set; }
    public DefinedType? Type { get; set; }
}
