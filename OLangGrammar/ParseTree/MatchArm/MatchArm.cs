using Lexing;
using OLangGrammar.ParseTree.Expression;
using OLangTokens.Tokens;

namespace OLangGrammar.ParseTree.MatchArm;

public class MatchArm(IdentifierToken enumName, IdentifierToken variantName, IExpression value) : IMatchArm
{
    public IdentifierToken EnumName = enumName;
    public IdentifierToken VariantName = variantName;
    public IExpression Value = value;
    public SourceSpan Span { get; set; }
}