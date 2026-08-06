using Lexing;
using OLangGrammar.ParseTree.Expression;

namespace OLangGrammar.ParseTree.Stmt;

public class Exit(IExpression expression) : IStatement
{
    public IExpression Expression = expression;
    public SourceSpan Span { get; set; }
}