using Lexing;
using OLangGrammar.ParseTree.Expression;
using OLangGrammar.ParseTree.Type;
using OLangTokens.Tokens;

namespace OLangGrammar.ParseTree.Stmt;

public class Declaration(IType type, IdentifierToken identifier, IExpression expression) : IStatement
{
    public IType Type = type;
    public IdentifierToken Identifier = identifier;
    public IExpression Expression = expression;
    public SourceSpan Span { get; set; }
}