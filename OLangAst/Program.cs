using Lexing;

namespace OLangAst;

public class Program(List<ClassDeclaration> classDeclarations) : IAstNode
{
    public List<ClassDeclaration> ClassDeclarations = classDeclarations;
    public SourceSpan Span { get; set; }
}