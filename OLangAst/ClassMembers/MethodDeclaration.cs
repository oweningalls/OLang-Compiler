using Lexing;
using OLangAst.Miscellaneous;
using OLangAst.Statements;
using OLangAst.TypeSystem;

namespace OLangAst.ClassMembers;

public class MethodDeclaration(IVariableType? declaredType, string identifier, List<ParameterNode> parameters, Scope scope, bool isInstance) : IClassMember
{
    public IVariableType? DeclaredType = declaredType;
    public string Identifier = identifier;
    public List<ParameterNode> Parameters = parameters;
    public Scope Scope = scope;
    public bool IsInstance = isInstance;
    public DefinedType? ReturnType; 
    
    public SourceSpan Span { get; set; }
}