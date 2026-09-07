using Lexing;
using OLangGrammar.ParseTree.ClassDeclaration;

namespace OLangGrammar.ParseTree.ClassDeclarationList;

public class ClassDeclarationListWithClass(IClassDeclaration classDeclaration, IClassDeclarationListNode classDeclarationList) : IClassDeclarationListNode
{
    public IClassDeclaration ClassDeclaration = classDeclaration;
    public IClassDeclarationListNode ClassDeclarationList = classDeclarationList;
    public SourceSpan Span { get; set; }
}