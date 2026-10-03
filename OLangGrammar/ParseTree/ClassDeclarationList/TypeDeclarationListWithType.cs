using Lexing;
using OLangGrammar.ParseTree.TypeDeclaration;

namespace OLangGrammar.ParseTree.ClassDeclarationList;

public class TypeDeclarationListWithType(ITypeDeclaration typeDeclaration, ITypeDeclarationListNode typeDeclarationList) : ITypeDeclarationListNode
{
    public ITypeDeclaration TypeDeclaration = typeDeclaration;
    public ITypeDeclarationListNode TypeDeclarationList = typeDeclarationList;
    public SourceSpan Span { get; set; }
}