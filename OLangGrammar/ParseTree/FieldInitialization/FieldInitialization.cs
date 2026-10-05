using Lexing;
using OLangGrammar.ParseTree.Expression;
using OLangTokens.Tokens;

namespace OLangGrammar.ParseTree.FieldInitialization;

public class FieldInitialization(IdentifierToken fieldName, IExpression value) : IFieldInitialization
{
    public IdentifierToken FieldName = fieldName;
    public IExpression Value = value;
    public SourceSpan Span { get; set; }
}