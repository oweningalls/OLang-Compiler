using Lexing;
using OLangGrammar.ParseTree.Expression;
using OLangGrammar.ParseTree.MatchArmList;

namespace OLangGrammar.ParseTree.Term;

public class MatchTerm(IExpression matchTarget, IMatchArmList matchArmList) : ITerm
{
    public IExpression MatchTarget = matchTarget;
    public IMatchArmList MatchArmList = matchArmList;
    public SourceSpan Span { get; set; }
}