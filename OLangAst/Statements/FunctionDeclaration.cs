using Lexing;
using OLangAst.Miscellaneous;
using OLangAst.TypeSystem;

namespace OLangAst.Statements;

public class FunctionDeclaration(IVariableType? declaredType, string identifier, List<ParameterNode> parameters, Scope scope)
    : IStatement
{
    public IVariableType? DeclaredType = declaredType;
    public string Identifier = identifier;
    public List<ParameterNode> Parameters = parameters;
    public Scope Scope = scope;
    public DefinedType? ReturnType; 
    public SourceSpan Span { get; set; }
}