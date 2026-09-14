using Lexing;
using OLangAst.Expressions;
using OLangAst.Miscellaneous;
using OLangAst.TypeSystem;

namespace OLangAst.Statements;

public class FunctionInvocation(string identifier, List<IExpression> arguments) : IStatement, IExpression
{
    public string Identifier = identifier;
    public List<IExpression> Arguments = arguments;
    public SourceSpan Span { get; set; }
    public DefinedType? Type { get; set; }
}