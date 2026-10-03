using Lexing;
using OLangGrammar.ParseTree.TypeDeclaration;

namespace OLangGrammar.ParseTree.ClassDeclarationList;

public class SingleTypeTypeList(ITypeDeclaration typeDeclaration) : ITypeDeclarationListNode
{
    public ITypeDeclaration TypeDeclaration = typeDeclaration;
    public SourceSpan Span { get; set; }
}