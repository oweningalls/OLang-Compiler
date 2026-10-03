using Lexing;
using OLangAst.TypeSystem;

namespace OLangAst;

public class Program(List<UsingStatement> usingStatements, List<ITypeDeclaration> typeDeclarations) : IAstNode
{
    public List<UsingStatement> UsingStatements = usingStatements;
    public List<ITypeDeclaration> TypeDeclarations = typeDeclarations;
    public SourceSpan Span { get; set; }
}