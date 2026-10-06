using Lexing;
using OLangGrammar.ParseTree.MatchArm;

namespace OLangGrammar.ParseTree.MatchArmList;

public class SingleMatchMatchArmList(IMatchArm matchArm) : IMatchArmList
{
    public IMatchArm MatchArm = matchArm;
    public SourceSpan Span { get; set; }
}