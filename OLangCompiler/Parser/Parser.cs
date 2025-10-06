using OLangCompiler.Parser.Nodes;
using OLangCompiler.Tokens;

namespace OLangCompiler.Parser;

public class Parser
{
    public ProgramNode ParseProgram(List<IToken> tokens)
    {
        _tokens = tokens;
        _currentIndex = 0;
        var statements = new List<IStatementNode>();
        while (Peek() != null)
        {
            statements.Add(ParseStatement());
        }

        return new ProgramNode(statements);
    }

    private IStatementNode ParseStatement()
    {
        if (Peek() == null)
        {
            throw ErrorHelper.UnexpectedEndOfInput(Peek(-1)!);
        }

        if (TryConsume<ExitToken>() != null)
        {
            var expression = ParseExpression();
            TryConsume<SemicolonToken>("Expected `;`");
            return new ExitStatement(expression);
        }

        if (TryConsume<ITypeToken>() is { } type)
        {
            var identifier = TryConsume<IdentifierToken>("Expected identifier");
            TryConsume<EqualsToken>("Expected `=`");
            var expression = ParseExpression();
            TryConsume<SemicolonToken>("Expected `;`");
            return new DeclarationStatement(identifier, expression, type.ExpType);
        }

        if (TryConsume<IdentifierToken>() is { } ident)
        {
            TryConsume<EqualsToken>("Expected `=`");
            var expression = ParseExpression();
            TryConsume<SemicolonToken>("Expected `;`");
            return new AssignmentStatement(ident, expression);
        }

        throw ErrorHelper.ExpectedValue("statement", Peek(-1)!);
    }

    private IExpressionNode ParseExpression()
    {
        var term = ParseTerm();
        if (TryConsume<PlusToken>() != null)
        {
            var expression = ParseExpression();

            return new AddExpression(term, expression);
        }

        if (TryConsume<MinusToken>() != null)
        {
            // TODO: this needs to be left associative
            var expression = ParseExpression();

            return new SubtractExpression(term, expression);
        }

        return new TermExpression(term);
    }

    private ITermNode ParseTerm()
    {
        if (Peek() == null)
        {
            throw ErrorHelper.UnexpectedEndOfInput(Peek(-1)!);
        }

        if (TryConsume<IntLiteralToken>() is {} ilt)
        {
            return new IntLiteralTerm(ilt.Value);
        }
        
        if (TryConsume<BoolLiteralToken>() is {} blt)
        {
            return new BoolLiteralTerm(blt.Value);
        }

        if (TryConsume<IdentifierToken>() is { } identifier)
        { 
            return new IdentifierTerm(identifier.Identifier);
        }

        if (TryConsume<LeftParenToken>() != null)
        {
            var expression = ParseExpression();
            TryConsume<RightParenToken>("Expected `)`");
            return new ParenTerm(expression);
        }
        
        throw ErrorHelper.ExpectedValue("term", Peek(-1)!);
    }
    
    private List<IToken>? _tokens;
    private int _currentIndex;
    private IToken? Peek(int offset = 0)
    {
        var index = _currentIndex + offset;
        if (index >= _tokens!.Count) return null;

        return _tokens[index];
    }

    private IToken Consume()
    {
        var ret = Peek();
        _currentIndex++;
        if (ret == null)
        {
            throw ErrorHelper.UnexpectedEndOfInput(Peek(-1)!);
        }

        return ret;
    }

    private T? TryConsume<T>() where T : class, IToken
    {
        if (Peek() is T)
        {
            return Consume() as T;
        }

        return null;
    }

    private T TryConsume<T>(string errorMessage) where T : class, IToken
    {
        var token = Peek() as T ?? throw ErrorHelper.UnexpectedEndOfInput(Peek(-1));
        Consume();
        return token;
    }
}