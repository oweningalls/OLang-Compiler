using Lexing;
using OLangGrammar.ParseTree.Expression;

namespace OLangGrammar.ParseTree.ArgumentList;

public class ExpressionArgumentList(IExpression expression) : IArgumentList
{
    public IExpression Expression = expression;
    public SourceSpan Span { get; set; }
}