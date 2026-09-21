using Lexing;

namespace OLangAst;

public class Program(List<UsingStatement> usingStatements, List<ClassDeclaration> classDeclarations) : IAstNode
{
    public List<UsingStatement> UsingStatements = usingStatements;
    public List<ClassDeclaration> ClassDeclarations = classDeclarations;
    public SourceSpan Span { get; set; }
}