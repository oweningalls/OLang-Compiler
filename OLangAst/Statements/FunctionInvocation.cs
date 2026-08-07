using Lexing;
using OLangAst.Expressions;

namespace OLangAst.Statements;

public class FunctionInvocation(string identifier, List<IExpression> arguments) : IStatement, IExpression
{
    public string Identifier = identifier;
    public List<IExpression> Arguments = arguments;
    public SourceSpan Span { get; set; }
}