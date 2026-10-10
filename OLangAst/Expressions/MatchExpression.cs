using Lexing;
using OLangAst.TypeSystem;

namespace OLangAst.Expressions;

public class MatchExpression(IExpression matchTarget, List<MatchArm> matchArms) : IExpression
{
    public IExpression MatchTarget = matchTarget;
    public List<MatchArm> MatchArms = matchArms;
    public SourceSpan Span { get; set; }
    public ConcreteType? Type { get; set; }
}