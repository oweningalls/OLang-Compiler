using Lexing;
using OLangGrammar.UsingStatement;

namespace OLangGrammar.ParseTree.UsingList;

public class UsingListWithStatement(IUsingStatement usingStatement, IUsingList usingList) : IUsingList
{
    public IUsingStatement UsingStatement = usingStatement;
    public IUsingList UsingList = usingList;
    public SourceSpan Span { get; set; }
}