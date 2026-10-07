using Lexing;
using OLangGrammar.ParseTree.Expression;
using OLangGrammar.ParseTree.IdentifierList;
using OLangTokens.Tokens;

namespace OLangGrammar.ParseTree.MatchArm;

public class DestructuringMatchArm(IdentifierToken enumName, IdentifierToken variantName, IIdentifierList identifierList, IExpression value) : IMatchArm
{
    public IdentifierToken EnumName = enumName;
    public IdentifierToken VariantName = variantName;
    public IIdentifierList IdentifierList = identifierList;
    public IExpression Value = value;
    public SourceSpan Span { get; set; }
}