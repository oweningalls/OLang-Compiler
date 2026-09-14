using Lexing;
using OLangAst.Miscellaneous;
using OLangAst.Statements;
using OLangAst.TypeSystem;

namespace OLangAst.ClassMembers;

public class MethodDeclaration(IVariableType? declaredType, string identifier, List<ParameterNode> parameters, Scope scope) : IClassMember
{
    public IVariableType? DeclaredType = declaredType;
    public string Identifier = identifier;
    public List<ParameterNode> Parameters = parameters;
    public Scope Scope = scope;
    public DefinedType? ReturnType; 
    
    public SourceSpan Span { get; set; }
}