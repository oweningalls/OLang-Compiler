using Lexing;
using OLangAst.Miscellaneous;
using OLangAst.Statements;

namespace OLangAst.ClassMembers;

public class MethodDeclaration(IVariableType? type, string identifier, List<Parameter> parameters, Scope scope) : IClassMember
{
    public IVariableType? Type = type;
    public string Identifier = identifier;
    public List<Parameter> Parameters = parameters;
    public Scope Scope = scope;
    public SourceSpan Span { get; set; }
}