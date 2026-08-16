using Lexing;
using OLangAst.Miscellaneous;

namespace OLangAst.Statements;

public class FunctionDeclaration(IVariableType? type, string identifier, List<Parameter> parameters, Scope scope)
    : IStatement
{
    public IVariableType? Type = type;
    public string Identifier = identifier;
    public List<Parameter> Parameters = parameters;
    public Scope Scope = scope;
    public SourceSpan Span { get; set; }
}