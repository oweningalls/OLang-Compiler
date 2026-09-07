using Lexing;
using OLangGrammar.ParseTree.ClassDeclaration;

namespace OLangGrammar.ParseTree.ClassDeclarationList;

public class SingleClassClassList(IClassDeclaration classDeclaration) : IClassDeclarationListNode
{
    public IClassDeclaration ClassDeclaration = classDeclaration;
    public SourceSpan Span { get; set; }
}