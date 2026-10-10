using Lexing;
using OLangAst.Expressions;
using OLangAst.TypeSystem;

namespace OLangAst.Statements;

public class MethodInvocation : IStatement, IExpression
{
    public bool IsInstance => Expression != null;
    public string? ClassName;
    public IExpression? Expression;
    public string Identifier;
    public List<IExpression> Arguments;
    public ConcreteType? SourceType;

    public MethodInvocation(string className, string identifier, List<IExpression> arguments)
    {
        ClassName = className;
        Identifier = identifier;
        Arguments = arguments;
    }
    
    public MethodInvocation(IExpression expression, string identifier, List<IExpression> arguments)
    {
        Expression = expression;
        Identifier = identifier;
        Arguments = arguments;
    }

    public SourceSpan Span { get; set; }
    public ConcreteType? Type { get; set; }
}
