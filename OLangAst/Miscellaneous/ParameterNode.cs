using Lexing;
using OLangAst.TypeSystem;

namespace OLangAst.Miscellaneous;

public class ParameterNode(string identifier, IVariableType declaredType) : IAstNode
{
    public IVariableType DeclaredType = declaredType;
    public string Identifier = identifier;
    public ConcreteType? Type;
    public SourceSpan Span { get; set; }
}