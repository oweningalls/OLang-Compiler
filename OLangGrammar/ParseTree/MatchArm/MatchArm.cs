using Lexing;
using OLangGrammar.ParseTree.Expression;
using OLangGrammar.ParseTree.TypeArgumentList;
using OLangTokens.Tokens;

namespace OLangGrammar.ParseTree.MatchArm;

public class MatchArm(IdentifierToken enumName, ITypeArgumentList? typeArgumentList, IdentifierToken variantName, IExpression value) : IMatchArm
{
    public IdentifierToken EnumName = enumName;
    public ITypeArgumentList? TypeArgumentList = typeArgumentList;
    public IdentifierToken VariantName = variantName;
    public IExpression Value = value;
    public SourceSpan Span { get; set; }
}