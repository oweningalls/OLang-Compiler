using Lexing;
using OLangGrammar.ParseTree.ClassDeclarationList;
using OLangGrammar.ParseTree.UsingList;

namespace OLangGrammar.ParseTree.Prog;

public class ProgramNode(IUsingList usingList, IClassDeclarationListNode classDeclarationList) : IProgramNode
{
    public IUsingList UsingList = usingList;
    public IClassDeclarationListNode ClassDeclarationList = classDeclarationList;
    public SourceSpan Span { get; set; }
}