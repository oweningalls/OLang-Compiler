using Lexing;
using OLangGrammar.ParseTree.MatchArm;

namespace OLangGrammar.ParseTree.MatchArmList;

public class ContinuedMatchMatchArmList(IMatchArm matchArm, IMatchArmList matchArmList) : IMatchArmList
{
    public IMatchArm MatchArm = matchArm;
    public IMatchArmList MatchArmList = matchArmList;
    public SourceSpan Span { get; set; }
}