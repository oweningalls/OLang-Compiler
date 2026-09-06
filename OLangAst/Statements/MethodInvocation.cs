using Lexing;
using OLangAst.Expressions;
using OLangAst.Miscellaneous;

namespace OLangAst.Statements;

public class MethodInvocation(IExpression expression, string identifier, List<IExpression> arguments) : IStatement, IExpression
{
    public IExpression Expression = expression;
    public string Identifier = identifier;
    public List<IExpression> Arguments = arguments;
    public SourceSpan Span { get; set; }
    public IVariableType? Type { get; set; }
}
