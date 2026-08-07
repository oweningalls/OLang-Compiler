using Lexing;
using OLangAst.Expressions;
using OLangAst.Miscellaneous;

namespace OLangAst.Statements;

public class VariableDeclarationStatement(IVariableType? type, string identifier, IExpression value) : IStatement
{
    public IVariableType? Type = type;
    public string Identifier = identifier;
    public IExpression Value = value;
    public SourceSpan Span { get; set; }
}