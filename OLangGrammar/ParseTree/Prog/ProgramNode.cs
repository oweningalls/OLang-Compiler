using Lexing;
using OLangGrammar.ParseTree.ClassDeclarationList;
using OLangGrammar.ParseTree.UsingList;

namespace OLangGrammar.ParseTree.Prog;

public class ProgramNode(IUsingList usingList, ITypeDeclarationListNode typeDeclarationList) : IProgramNode
{
    public IUsingList UsingList = usingList;
    public ITypeDeclarationListNode TypeDeclarationList = typeDeclarationList;
    public SourceSpan Span { get; set; }
}