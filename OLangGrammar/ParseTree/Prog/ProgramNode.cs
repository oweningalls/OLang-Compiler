using Lexing;
using OLangGrammar.ParseTree.ClassDeclarationList;
using OLangGrammar.ParseTree.StmtList;

namespace OLangGrammar.ParseTree.Prog;

public class ProgramNode(IClassDeclarationListNode classDeclarationList) : IProgramNode
{
    public IClassDeclarationListNode ClassDeclarationList = classDeclarationList;
    public SourceSpan Span { get; set; }
}